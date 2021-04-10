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

        public DataEnricherExecutor(IBankSyncLogger logger)
        {
            this.logger = logger;
        }

        private readonly List<IBankDataEnricher> enrichers = new List<IBankDataEnricher>();

        public void LoadEnrichers(BankSyncConfig config)
        {
            var allegroConfig = config.Services.FirstOrDefault(x => x.Name == "Allegro");

            if (allegroConfig != null)
            {
                this.enrichers.Add(new AllegroBankDataEnricher(allegroConfig,logger));
            }
        }

        public async Task EnrichData(BankDataSheet data, DateTime startTime, DateTime endTime)
        {
            foreach (IBankDataEnricher bankDataEnricher in this.enrichers)
            {
                try
                {
                    await bankDataEnricher.Enrich(data, startTime, endTime);
                }
                catch (Exception ex)
                {
                    this.logger.Warning($"{bankDataEnricher.GetType().Name} - Failed to enrich data. {ex.Message}");
                }
            }
        }
    }
}