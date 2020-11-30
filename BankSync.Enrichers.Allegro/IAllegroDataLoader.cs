using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankSync.Enrichers.Allegro
{
    internal interface IAllegroDataLoader
    {
        Task<List<AllegroDataContainer>> LoadAllData(DateTime oldestEntry);
    }
}