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
        private List<string> processedUsers = new List<string>();
        private int currentUserIndex = 0;
        private DateTime oldestEntryToDownload;
        private Action<List<AllegroDataContainer>> allUsersCompletionCallback;
        private List<AllegroDataContainer> allUsersData;


        public AllegroDataLoader(ServiceConfig config, IAllegroDataDownloader dataLoader, IBankSyncLogger logger)
        {
            this.config = config;
            this.dataLoader = dataLoader;
            this.logger = logger;
        }

        public async Task LoadAllData(DateTime oldestEntry,bool getFreshEnrichmentData, Action<List<AllegroDataContainer>> completionCallback)
        {
            this.oldestEntryToDownload = oldestEntry;
            this.allUsersCompletionCallback = completionCallback;
            this.allUsersData = new List<AllegroDataContainer>();

            await GetDataForUser(getFreshEnrichmentData);
        }

        private async Task GetDataForUser(bool getFreshEnrichmentData)
        {
            ServiceUser serviceUser = this.config.Users[currentUserIndex];
            OldDataManager oldDataManager = new OldDataManager(serviceUser, this.logger);
            AllegroDataContainer oldData = oldDataManager.GetOldData();

            DateTime oldestEntryAdjusted = AdjustOldestEntryToDownloadBasedOnOldData(this.oldestEntryToDownload, oldData);

            if (getFreshEnrichmentData)
            {
                await this.dataLoader.GetData(serviceUser, oldestEntryAdjusted, async newData
                    => await HandleDownloadedData(serviceUser, getFreshEnrichmentData, oldDataManager, newData, oldData, allUsersData)
                );
            }
            else
            {
                await HandleDownloadedData(serviceUser, getFreshEnrichmentData, oldDataManager, null, oldData, allUsersData);
            }
            
        }

        private async Task HandleDownloadedData(ServiceUser serviceUser, bool getFreshEnrichmentData, OldDataManager oldDataManager,
            AllegroDataContainer newData,
            AllegroDataContainer oldData, List<AllegroDataContainer> allUsersData)
        {
            oldDataManager.StoreData(newData);

            AllegroDataContainer consolidatedData = AllegroDataContainer.Consolidate(GetConsolidationInput(newData, oldData));
            

            allUsersData.Add(consolidatedData);

            if (!processedUsers.Contains(serviceUser.UserName))
            {
                processedUsers.Add(serviceUser.UserName);
                this.logger.Info($"Processed user {serviceUser.UserName}");
                if (processedUsers.Count == this.config.Users.Count)
                {
                    allUsersCompletionCallback(allUsersData);
                }
                else
                {
                    this.currentUserIndex++;
                    await this.GetDataForUser(getFreshEnrichmentData);
                }
            }

        }

        private List<AllegroDataContainer> GetConsolidationInput(AllegroDataContainer newData, AllegroDataContainer oldData)
        {
            var list = new List<AllegroDataContainer>();

            if (oldData != null)
            {
                list.Add(oldData);
            }
            if (newData != null)
            {
                list.Add(newData);
             }
            return list;
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