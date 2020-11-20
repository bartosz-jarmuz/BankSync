// -----------------------------------------------------------------------
//  <copyright file="AllegroDataContainer.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using BankSync.Enrichers.Allegro.Model;
using Newtonsoft.Json;

namespace BankSync.Enrichers.Allegro
{
    internal class AllegroDataContainer 
    {
        public AllegroData Model { get; internal set; }
        public string ServiceUserName { get; internal set; }

        public DateTime OldestEntry { get; internal set; }
        public DateTime NewestEntry { get; internal set; }

        public AllegroDataContainer(AllegroData model, string serviceUserName)
        {
            this.Model = model;
            this.ServiceUserName = serviceUserName;
            this.AssignTimeRange();
        }

        private void AssignTimeRange()
        {
            List<DateTime> allDates = GetAllDates(this.Model);

            this.NewestEntry = allDates.First();
            this.OldestEntry = allDates.Last();
        }

        public static DateTime GetOldestDate(AllegroData model)
        {
            List<DateTime> allDates = GetAllDates(model);
            return allDates.Last();
        }

        public static AllegroDataContainer Consolidate(List<AllegroDataContainer> dataList)
        {
            dataList = dataList.Where(x => x != null).ToList();
            

            AllegroData first = dataList.First().Model;
            IEnumerable<Myorder> allOrders = dataList.SelectMany(x => x.Model.parameters.myorders.myorders);
            Myorder[] distinct = allOrders.GroupBy(x => x.id).Select(g => g.First()).OrderByDescending(x => x.orderDate).ToArray();

            first.parameters.myorders.myorders = distinct;
            return new AllegroDataContainer(first, dataList.First().ServiceUserName);
        }

        public static List<AllegroDataContainer> SplitPerMonth( AllegroDataContainer container)
        {
            var list = new List<AllegroDataContainer>();
            IEnumerable<IGrouping<string, Myorder>> groupings =
                container.Model.parameters.myorders.myorders.GroupBy(x => x.orderDate.ToString("yyyy-MM"));

            foreach (IGrouping<string, Myorder> grouping in groupings)
            {
                AllegroDataContainer clone = container.Clone();
                clone.Model.parameters.myorders.myorders = grouping.ToArray();
                clone.Model.parameters.myorders.total = clone.Model.parameters.myorders.myorders.Length;
                clone.AssignTimeRange();
                list.Add(clone);
            }

            return list;
        }


        private static List<DateTime> GetAllDates(AllegroData model)
        {
            return  model.parameters.myorders.myorders.Select(x => Convert.ToDateTime(x.orderDate))
                .OrderByDescending(x => x).ToList();
        }


        public AllegroDataContainer Clone()
        {
            var serialized = JsonConvert.SerializeObject(this);

            return JsonConvert.DeserializeObject<AllegroDataContainer>(serialized);
        }
    }
}