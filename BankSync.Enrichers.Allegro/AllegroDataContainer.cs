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
    public class AllegroDataContainer 
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
            if (this.Model?.myorders != null)
            {

                List<DateTime> allDates = GetAllDates(this.Model);

                this.NewestEntry = allDates.First();
                this.OldestEntry = allDates.Last();
            }
        }

        public static DateTime GetOldestDate(AllegroData model)
        {
            if (model.myorders == null)
            {
                return DateTime.MinValue;
                
            }
            List<DateTime> allDates = GetAllDates(model);
            return allDates.Last();
        }

        public static AllegroDataContainer Consolidate(List<AllegroDataContainer> dataList)
        {
            foreach (var container in dataList)
            {
                var x = container.Model.myorders.orderGroups.Where(g => g.myorders.Count() > 2).ToList();
                if (x.Any())
                {

                }
            }

            //data might be provided in batches, so consolidate all into first items' data
            dataList = dataList.Where(x => x?.Model.myorders != null).ToList();
            

            AllegroData consolidationTarget = dataList.First().Model;
            IEnumerable<OrderGroup> allOrderGroups = dataList.SelectMany(x => x.Model.myorders.orderGroups);
            OrderGroup[] distinct = allOrderGroups.GroupBy(x => x.groupId).Select(g => g.First()).ToArray();

            consolidationTarget.myorders.orderGroups = distinct;
            return new AllegroDataContainer(consolidationTarget, dataList.First().ServiceUserName);
        }

        public static List<AllegroDataContainer> SplitPerMonth( AllegroDataContainer container)
        {
          
            var list = new List<AllegroDataContainer>();
            IEnumerable<IGrouping<string, OrderGroup>> groupings =
                container.Model.myorders.orderGroups.GroupBy(x => x.myorders.First().orderDate.ToString("yyyy-MM"));

            foreach (IGrouping<string, OrderGroup> grouping in groupings)
            {
                var unmatching = grouping.Where(x => x.myorders.Any(o => o.orderDate.ToString("yyyy-MM") != grouping.Key));
                if (unmatching.Any())
                {

                }

                AllegroDataContainer clone = container.Clone();
                clone.Model.myorders.orderGroups = grouping.ToArray();
                clone.Model.myorders.total = clone.Model.myorders.total;
                clone.AssignTimeRange();
                list.Add(clone);
            }

            return list;
        }


        private static List<DateTime> GetAllDates(AllegroData model)
        {
            return  model.myorders.orderGroups.SelectMany(group=> group.myorders.Select(order => Convert.ToDateTime(order.orderDate)))
                .OrderByDescending(x => x).ToList();
        }


        public AllegroDataContainer Clone()
        {
            var serialized = JsonConvert.SerializeObject(this);

            return JsonConvert.DeserializeObject<AllegroDataContainer>(serialized);
        }
    }
}