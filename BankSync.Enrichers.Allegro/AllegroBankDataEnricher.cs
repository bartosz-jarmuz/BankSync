// -----------------------------------------------------------------------
//  <copyright file="AllegroBankDataEnricher.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BankSync.Config;
using BankSync.Enrichers.Allegro.Model;
using BankSync.Logging;
using BankSync.Model;

namespace BankSync.Enrichers.Allegro
{
    public class AllegroBankDataEnricher : IBankDataEnricher
    {
        internal const string NierozpoznanyZakup = "Nierozpoznany zakup";
        private readonly IBankSyncLogger logger;
        private readonly IAllegroDataLoader dataLoader;

        public AllegroBankDataEnricher(ServiceConfig config, IBankSyncLogger logger)
        {
            this.logger = logger;
            this.dataLoader = new AllegroDataLoader(config,logger);
        }
        
        internal AllegroBankDataEnricher(IBankSyncLogger logger, IAllegroDataLoader dataLoader)
        {
            this.logger = logger;
            this.dataLoader = dataLoader;
        }
       
        public async Task Enrich(BankDataSheet data, DateTime startTime, DateTime endTime)
        {
            List<AllegroDataContainer> allData = await this.dataLoader.LoadAllData(startTime);
            List<BankEntry> allUpdatedEntries = new List<BankEntry>();
            RefundEnricher refunds = new RefundEnricher(this.logger);
            PurchaseEnricher purchases = new PurchaseEnricher(this.logger);
            foreach (BankEntry entry in data.Entries)
            {
                List<BankEntry> newEntries = ExtractAllegroEntries(entry, refunds, allData, purchases);
                allUpdatedEntries.AddRange(newEntries);
            }
            data.Entries = allUpdatedEntries;
        }

        private List<BankEntry> ExtractAllegroEntries(BankEntry entry, RefundEnricher refunds, List<AllegroDataContainer> allData,
            PurchaseEnricher purchases)
        {
            if (IsAllegro(entry))
            {
                List<BankEntry> entriesForThisPayment = new List<BankEntry>();

                decimal buyerPaidAmount = 0;
                if (entry.Amount > 0)
                {
                    refunds.EnrichAllegroEntry(entry, allData, entriesForThisPayment, out buyerPaidAmount);
                }
                else
                {
                    purchases.EnrichAllegroEntry(entry, allData, entriesForThisPayment, out buyerPaidAmount);
                    if (buyerPaidAmount != entry.Amount)
                    {
                        if (!entry.Note.Contains(NierozpoznanyZakup))
                        {

                            this.logger.Error("ERROR", new InvalidOperationException(
                                "Incorrect result of Allegro entry enriching. Original entry will be returned." +
                                $"The sum of {entriesForThisPayment.Count} new entries ({buyerPaidAmount}) plus sum of discounts ({buyerPaidAmount}) is different than the original entry amount. {entry}." +
                                $"New entries: {string.Join("\r\n", entriesForThisPayment.Select(x => x.ToString()))}"));
                            return new List<BankEntry>() {entry};
                        }

                    }
                }

               

                return entriesForThisPayment;
            }
            else
            {
                //that's not Allegro entry, but needs to be preserved on the list
                return new List<BankEntry>(){entry};
            }
        }

        private static bool IsAllegro(BankEntry entry)
        {
            bool value = entry.Recipient.Contains("allegro.pl", StringComparison.OrdinalIgnoreCase) 
                         || entry.Recipient.Contains("PAYU*ALLEGRO", StringComparison.OrdinalIgnoreCase)
                         || (entry.Recipient.Contains("PAYPRO", StringComparison.OrdinalIgnoreCase) && entry.Note != null && entry.Note.Contains("allegro", StringComparison.OrdinalIgnoreCase))
                         || (entry.Recipient.Contains("PAYU", StringComparison.OrdinalIgnoreCase) && entry.Note != null && entry.Note.Contains("allegro", StringComparison.OrdinalIgnoreCase))
                         ;
            if (value)
            {
                return true;
            }
        
            value = entry.Note?.Contains("allegro", StringComparison.OrdinalIgnoreCase)??false;
            
            return value;
        }

      

    }
}