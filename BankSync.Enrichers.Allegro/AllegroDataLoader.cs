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
        private readonly IAllegroDataDownloader dataLoader;
        private readonly IBankSyncLogger logger;
        private readonly object allUsersProcessedCheckingLock = new object();
        private int numberOfUsersProcessed = 0;

        public AllegroDataLoader(ServiceConfig config, IAllegroDataDownloader dataLoader, IBankSyncLogger logger)
        {
            this.config = config;
            this.dataLoader = dataLoader;
            this.logger = logger;
        }

        public void LoadAllData(DateTime oldestEntry, Action<List<AllegroDataContainer>> completionCallback)
        {
            List<AllegroDataContainer> allUsersData = new List<AllegroDataContainer>();

            foreach (ServiceUser serviceUser in this.config.Users)
            {
                OldDataManager oldDataManager = new OldDataManager(serviceUser, this.logger);
                AllegroDataContainer oldData = oldDataManager.GetOldData();

                DateTime oldestEntryAdjusted = AdjustOldestEntryToDownloadBasedOnOldData(oldestEntry, oldData);

                this.dataLoader.GetData(serviceUser, oldestEntryAdjusted, 
                    newData => HandleDownloadedData(oldDataManager, newData, oldData, allUsersData, completionCallback)
                    );
            }
        }

        private void HandleDownloadedData(OldDataManager oldDataManager, AllegroDataContainer newData,
            AllegroDataContainer oldData, List<AllegroDataContainer> allUsersData,
            Action<List<AllegroDataContainer>> completionCallback)
        {
            oldDataManager.StoreData(newData);

            AllegroDataContainer consolidatedData =
                AllegroDataContainer.Consolidate(new List<AllegroDataContainer>() {newData, oldData});

            allUsersData.Add(consolidatedData);

            lock (allUsersProcessedCheckingLock)
            {
                numberOfUsersProcessed++;
                if (numberOfUsersProcessed == this.config.Users.Count)
                {
                    completionCallback(allUsersData);
                }
            }

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