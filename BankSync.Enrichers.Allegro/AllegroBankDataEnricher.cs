// -----------------------------------------------------------------------
//  <copyright file="AllegroBankDataEnricher.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankSync.Config;
using BankSync.Enrichers.Allegro.Model;
using BankSync.Model;

namespace BankSync.Enrichers.Allegro
{
    public class AllegroBankDataEnricher : IBankDataEnricher
    {
        private readonly ServiceConfig config;

        public AllegroBankDataEnricher(ServiceConfig config)
        {
            this.config = config;
        }

        private async Task<List<AllegroDataContainer>> LoadAllData()
        {
            List<AllegroDataContainer> allegroData = new List<AllegroDataContainer>();

            foreach (ServiceUser serviceUser in this.config.Users)
            {
                AllegroData model = await new AllegroDataDownloader(serviceUser).GetData();
                AllegroDataContainer container = new AllegroDataContainer(model, serviceUser.UserName);
                allegroData.Add(container);
            }

            return allegroData;
        }

        public async Task Enrich(WalletDataSheet data)
        {
            List<AllegroDataContainer> allData = await this.LoadAllData();
            var updatedEntries = new List<WalletEntry>();

            for (int index = 0; index < data.Entries.Count; index++)
            {
                var entry = data.Entries[index];

                if (IsAllegro(entry))
                {
                    List<Myorder> allegroEntries = this.GetRelevantEntry(entry, allData);
                    if (allegroEntries != null && allegroEntries.Any())
                    {
                        foreach (Myorder allegroEntry in allegroEntries)
                        {
                            foreach (Offer offer in allegroEntry.offers)
                            {
                                WalletEntry newEntry = WalletEntry.Clone(entry);
                                newEntry.Amount = Convert.ToDecimal(offer.offerPrice.amount) * -1;
                                newEntry.Note = offer.title;
                                newEntry.Recipient = "allegro.pl - " + allegroEntry.seller.login;
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
                else
                {
                    //that's either not Allegro entry or not entry of this person, but needs to be preserved on the list
                    updatedEntries.Add(entry);
                }
            }

            data.Entries = updatedEntries;
        }

        private static bool IsAllegro(WalletEntry entry)
        {
            if (entry.Recipient.Contains("PAYU*ALLEGRO", StringComparison.OrdinalIgnoreCase))
            {

            }
            return (entry.Recipient.Contains("allegro.pl", StringComparison.OrdinalIgnoreCase) || entry.Recipient.Contains("PAYU*ALLEGRO", StringComparison.OrdinalIgnoreCase));
        }

        private List<Myorder> GetRelevantEntry(WalletEntry entry, List<AllegroDataContainer> allegroDataContainers)
        {
            AllegroData model = allegroDataContainers.FirstOrDefault(x => x.ServiceUserName == entry.Payer)?.Model;
            if (model != null)
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
            else
            {
                return null;
            }
        }
    }
}