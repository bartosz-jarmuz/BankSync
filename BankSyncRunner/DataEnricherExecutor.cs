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
using BankSync.Model;

namespace BankSyncRunner
{
    public class DataEnricherExecutor
    {
        private readonly List<IBankDataEnricher> enrichers = new List<IBankDataEnricher>();

        public void LoadEnrichers(BankSyncConfig config)
        {

            var allegroConfig = config.Services.FirstOrDefault(x => x.Name == "Allegro");

            if (allegroConfig != null)
            {
                this.enrichers.Add(new AllegroBankDataEnricher(allegroConfig));
            }
        }

        public async Task EnrichData(WalletDataSheet data, DateTime startTime, DateTime endTime)
        {
            foreach (IBankDataEnricher bankDataEnricher in this.enrichers)
            {
                await bankDataEnricher.Enrich(data, startTime, endTime);
            }
        }
    }
}