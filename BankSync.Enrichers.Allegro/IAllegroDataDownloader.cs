using System;
using System.Threading.Tasks;
using BankSync.Config;

namespace BankSync.Enrichers.Allegro
{
    public interface IAllegroDataDownloader
    {
        Task GetData(ServiceUser userConfig, DateTime oldestEntry, Action<AllegroDataContainer> completionCallback);
    }
}