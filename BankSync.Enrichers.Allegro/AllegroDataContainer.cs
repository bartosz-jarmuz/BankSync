// -----------------------------------------------------------------------
//  <copyright file="AllegroDataContainer.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using BankSync.Enrichers.Allegro.Model;

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
            var allDates = GetAllDates(this.Model);

            this.NewestEntry = allDates.First();
            this.OldestEntry = allDates.Last();
        }

        public static DateTime GetOldestDate(AllegroData model)
        {
            var allDates = GetAllDates(model);
            return allDates.Last();
        }

        private static List<DateTime> GetAllDates(AllegroData model)
        {
            return  model.parameters.myorders.myorders.Select(x => Convert.ToDateTime(x.orderDate))
                .OrderByDescending(x => x).ToList();
        }
    }
}