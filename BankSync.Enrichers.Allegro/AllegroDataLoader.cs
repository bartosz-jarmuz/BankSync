using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankSync.Config;
using BankSync.Logging;

namespace BankSync.Enrichers.Allegro
{
    internal class AllegroDataLoader : IAllegroDataLoader
    {
        private readonly ServiceConfig config;
        private readonly IBankSyncLogger logger;

        public AllegroDataLoader(ServiceConfig config, IBankSyncLogger logger)
        {
            this.config = config;
            this.logger = logger;
        }

        public async Task<List<AllegroDataContainer>> LoadAllData(DateTime oldestEntry)
        {
            List<AllegroDataContainer> allUsersData = new List<AllegroDataContainer>();

            foreach (ServiceUser serviceUser in this.config.Users)
            {
                OldDataManager oldDataManager = new OldDataManager(serviceUser, this.logger);
                AllegroDataContainer oldData = oldDataManager.GetOldData();

                DateTime oldestEntryAdjusted = AdjustOldestEntryToDownloadBasedOnOldData(oldestEntry, oldData);

                AllegroDataContainer newData = await new AllegroDataDownloader(serviceUser, this.logger).GetData(oldestEntryAdjusted);
                oldDataManager.StoreData(newData);

                AllegroDataContainer consolidatedData =  AllegroDataContainer.Consolidate(new List<AllegroDataContainer>() { newData, oldData });

                allUsersData.Add(consolidatedData);
            }

            return allUsersData;
        }
        
        /// <summary>
        /// Don't download old data if older or equal data is already stored
        /// </summary>
        /// <param name="oldestEntry"></param>
        /// <param name="oldData"></param>
        /// <returns></returns>
        private static DateTime AdjustOldestEntryToDownloadBasedOnOldData(DateTime oldestEntry, AllegroDataContainer oldData)
        {
            if (oldData != null)
            {
                if (oldData.OldestEntry <= oldestEntry)
                {
                    oldestEntry = oldData.NewestEntry;
                }
            }

            return oldestEntry;
        }

    }
}