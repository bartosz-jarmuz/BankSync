// -----------------------------------------------------------------------
//  <copyright file="DataEnricherExecutor.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Config;
using BankSync.Enrichers.Allegro;
using BankSync.Exceptions;
using BankSync.Logging;
using BankSync.Model;

namespace BankSyncRunner
{
    public class DataEnricherExecutor
    {
        private readonly IBankSyncLogger logger;
        private readonly IAllegroDataDownloader dataDownloader;

        public DataEnricherExecutor(IBankSyncLogger logger, IAllegroDataDownloader dataDownloader)
        {
            this.logger = logger;
            this.dataDownloader = dataDownloader;
        }

        private readonly List<IBankDataEnricher> enrichers = new List<IBankDataEnricher>();

        public void LoadEnrichers(BankSyncConfig config)
        {
            var allegroConfig = config.Services.FirstOrDefault(x => x.Name == "Allegro");

            if (allegroConfig != null)
            {
                this.enrichers.Add(new AllegroBankDataEnricher(allegroConfig, dataDownloader, logger));
            }
        }

        public void EnrichData(BankDataSheet data, DateTime startTime, DateTime endTime, Action<BankDataSheet> completionCallback)
        {
            foreach (IBankDataEnricher bankDataEnricher in this.enrichers)
            {
                try
                {
                    bankDataEnricher.Enrich(data, startTime, endTime, completionCallback);
                }
                catch (Exception ex)
                {
                    this.logger.Warning($"{bankDataEnricher.GetType().Name} - Failed to enrich data. {ex.Message}");
                    throw;
                }
            }
        }
    }
}