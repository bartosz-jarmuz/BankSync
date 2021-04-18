using System;
using System.Threading.Tasks;
using BankSync.Config;

namespace BankSync.Enrichers.Allegro
{
    public interface IAllegroDataDownloader
    {
        void GetData(ServiceUser userConfig, DateTime oldestEntry, Action<AllegroDataContainer> completionCallback);
    }
}