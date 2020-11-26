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
using BankSync.Logging;
using BankSync.Model;

namespace BankSyncRunner
{
    public class DataEnricherExecutor
    {
        private readonly List<IBankDataEnricher> enrichers = new List<IBankDataEnricher>();

        public void LoadEnrichers(BankSyncConfig config, IBankSyncLogger logger)
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
                await bankDataEnricher.Enrich(data, startTime, endTime);
            }
        }
    }
}