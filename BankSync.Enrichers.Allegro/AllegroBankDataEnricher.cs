// -----------------------------------------------------------------------
//  <copyright file="AllegroBankDataEnricher.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankSync.Enrichers.Allegro.Model;
using BankSync.Model;

namespace BankSync.Exporters.Allegro
{
    public class AllegroBankDataEnricher : IBankDataEnricher
    {
        private readonly string owner;
        private readonly string manualDataFile;

        public AllegroBankDataEnricher(string owner)
        {
            this.owner = owner;
            if (this.owner == "Bartek")
            {
                this.manualDataFile = @"C:\Users\bjarmuz\Documents\BankSync\allegro_bartek.json";
            }
            else if (this.owner == "Justyna")
            {
                this.manualDataFile = @"C:\Users\bjarmuz\Documents\BankSync\allegro_justyna.json";
            }
            else
            {
                throw new NotImplementedException("This is very much WIP");
            }
        }

        public async Task Enrich(WalletDataSheet data)
        {
            AllegroData model = await new AllegroDataDownloader().GetData(this.manualDataFile);

            var updatedEntries = new List<WalletEntry>();

            for (int index = 0; index < data.Entries.Count; index++)
            {
                var entry = data.Entries[index];
                if (entry.Recipient.Contains("allegro.pl", StringComparison.OrdinalIgnoreCase) && entry.Payer == this.owner)
                {
                    List<Myorder> allegroEntries = GetRelevantEntry(model, entry);

                    foreach (Myorder allegroEntry in allegroEntries)
                    {
                        foreach (Offer offer in allegroEntry.offers)
                        {
                            WalletEntry newEntry = WalletEntry.Clone(entry);
                            newEntry.Amount = Convert.ToDecimal(offer.offerPrice.amount) * -1;
                            newEntry.Note = offer.title;
                            updatedEntries.Add(newEntry);
                        }
                    }
                }
                else
                {
                    //that's either not Allegro entry or not entry of this person, but needs to be preserved on the list
                    updatedEntries.Add(entry);
                }
            }

            data.Entries = updatedEntries;
        }

        private static List<Myorder> GetRelevantEntry(AllegroData model, WalletEntry entry)
        {
            List<Myorder> allegroEntries = model.parameters.myorders.myorders
                .Where(x => x.payment.buyerPaidAmount.amount == entry.Amount.ToString().Trim('-')).ToList();
            if (allegroEntries.Count < 2)
            {
                return allegroEntries;
            }
            else
            {
                allegroEntries = allegroEntries.Where(x => x.payment.endDate.Date == entry.Date).ToList();
                if (allegroEntries.Count < 1)
                {
                    Console.WriteLine($"ERROR - TOO FEW ENTRIES WHEN RECOGNIZING ALLEGRO ENTRY FOR {entry.Note}");
                }
                return allegroEntries;
            }
        }
    }
}