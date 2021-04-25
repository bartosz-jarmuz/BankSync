using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankSync.Enrichers.Allegro
{
    internal interface IAllegroDataLoader
    {
        Task LoadAllData(DateTime oldestEntry, Action<List<AllegroDataContainer>> completionCallback);
    }
}