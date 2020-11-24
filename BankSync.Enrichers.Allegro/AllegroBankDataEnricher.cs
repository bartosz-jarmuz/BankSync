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

        private async Task<List<AllegroDataContainer>> LoadAllData(DateTime oldestEntry)
        {
            List<AllegroDataContainer> allUsersData = new List<AllegroDataContainer>();

            foreach (ServiceUser serviceUser in this.config.Users)
            {
                OldDataManager oldDataManager = new OldDataManager(serviceUser);
                AllegroDataContainer oldData = oldDataManager.GetOldData();

                DateTime oldestEntryAdjusted = AdjustOldestEntryToDownloadBasedOnOldData(oldestEntry, oldData);

                AllegroDataContainer newData = await new AllegroDataDownloader(serviceUser).GetData(oldestEntryAdjusted);
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


        public async Task Enrich(BankDataSheet data, DateTime startTime, DateTime endTime)
        {
            List<AllegroDataContainer> allData = await this.LoadAllData(startTime);
            List<BankEntry> updatedEntries = new List<BankEntry>();

            foreach (BankEntry entry in data.Entries)
            {
                if (IsAllegro(entry))
                {
                    this.EnrichAllegroEntry(entry, allData, updatedEntries);
                }
                else
                {
                    //that's either not Allegro entry or not entry of this person, but needs to be preserved on the list
                    updatedEntries.Add(entry);
                }
            }

            data.Entries = updatedEntries;
        }

        private void EnrichAllegroEntry(BankEntry entry, List<AllegroDataContainer> allData, List<BankEntry> updatedEntries)
        {
            List<Myorder> allegroEntries = this.GetRelevantEntry(entry, allData, out AllegroDataContainer container,
                out List<string> potentiallyRelatedOfferIds);
            if (allegroEntries != null && allegroEntries.Any())
            {
                foreach (Myorder allegroEntry in allegroEntries)
                {
                    for (int offerIndex = 0; offerIndex < allegroEntry.offers.Length; offerIndex++)
                    {
                        BankEntry newEntry = PrepareNewBankEntryForOffer(allegroEntry, offerIndex, entry, container);

                        updatedEntries.Add(newEntry);
                    }
                }
            }
            else
            {
                //that's either not Allegro entry or not entry of this person, but needs to be preserved on the list
                EnrichUnrecognizedAllegroOffer(entry, container, potentiallyRelatedOfferIds);
                updatedEntries.Add(entry);
            }
        }

        private static void EnrichUnrecognizedAllegroOffer(BankEntry entry, AllegroDataContainer container, List<string> potentiallyRelatedOfferIds)
        {
            if (entry.Amount > 0)
            {
                entry.Payer = "allegro.pl (Nierozpoznany sprzedawca)";
                entry.Recipient = container.ServiceUserName;
                if (potentiallyRelatedOfferIds != null && potentiallyRelatedOfferIds.Any())
                {
                    entry.Note = "Zwrot pasujący do ofert: " + string.Join(",", potentiallyRelatedOfferIds);
                }
                else
                {
                    entry.Note = "Nierozpoznany zwrot";
                }
            }
            else
            {
                if (entry.Note == entry.FullDetails)
                {
                    entry.Note = "Nierozpoznany zakup";
                }
                else
                {
                    entry.Note = "Nierozpoznany zakup - " + entry.Note;
                }
            }
        }

        private static BankEntry PrepareNewBankEntryForOffer(Myorder allegroEntry, int offerIndex, BankEntry entry,
            AllegroDataContainer container)
        {
            Offer offer = allegroEntry.offers[offerIndex];
            BankEntry newEntry = BankEntry.Clone(entry);
            if (entry.Amount < 0)
            {
                newEntry.Amount = Convert.ToDecimal(offer.offerPrice.amount) * -1;
            }
            else
            {
                newEntry.Amount = Convert.ToDecimal(offer.offerPrice.amount);
            }

            newEntry.Note = $"{offer.title} (Oferta {offer.id}, Przedmiot {offerIndex + 1}/{allegroEntry.offers.Length})";
            if (entry.Amount < 0)
            {
                newEntry.Recipient = "allegro.pl - " + allegroEntry.seller.login;
                if (string.IsNullOrEmpty(newEntry.Payer))
                {
                    newEntry.Payer = container.ServiceUserName;
                }
            }
            else
            {
                newEntry.Payer = "allegro.pl - " + allegroEntry.seller.login;
                newEntry.Recipient = container.ServiceUserName;
            }

            return newEntry;
        }

        private static bool IsAllegro(BankEntry entry)
        {
            var value = entry.Recipient.Contains("allegro.pl", StringComparison.OrdinalIgnoreCase) || entry.Recipient.Contains("PAYU*ALLEGRO", StringComparison.OrdinalIgnoreCase);
            if (value)
            {
                return true;
            }

            value = entry.Note?.Contains("allegro", StringComparison.OrdinalIgnoreCase)??false;
            
            return value;
        }

        private List<Myorder> GetRelevantEntry(BankEntry entry, List<AllegroDataContainer> allegroDataContainers, out AllegroDataContainer associatedContainer, out List<string> potentiallyRelatedOfferIds)
        {
            associatedContainer = allegroDataContainers.FirstOrDefault(x => x.ServiceUserName == entry.Payer);
            AllegroData model = associatedContainer?.Model;
            List<Offer> offersWhichMatchThePriceButNotTheDateBecauseTheyAreRefunds = null;

            List<Myorder> result = null;
            if (model != null)
            {
                result = GetAllegroEntries(entry, model, out _ );
            }

            if (result != null)
            {
                potentiallyRelatedOfferIds = null;
                return result;
            }
            else
            {

                //the payer can be empty or it can be somehow incorrect, but if we have an entry that matches the exact price and date... it's probably IT
                foreach (var container in allegroDataContainers)
                {
                    var entries = GetAllegroEntries(entry, container.Model, out List<Offer> offersMatchingPrice );
                    if (offersMatchingPrice != null && offersMatchingPrice.Any())
                    {
                        offersWhichMatchThePriceButNotTheDateBecauseTheyAreRefunds = new List<Offer>(offersMatchingPrice);
                    }
                    if (entries != null && entries.Any())
                    {
                        associatedContainer = container;
                        potentiallyRelatedOfferIds = null;
                        return entries;
                    }
                }
            }

            potentiallyRelatedOfferIds =
                offersWhichMatchThePriceButNotTheDateBecauseTheyAreRefunds?.Select(x => x.id).ToList(); 
            return null;
        }

        private static List<Myorder> GetAllegroEntries(BankEntry entry, AllegroData model, out List<Offer> offersMatchingPrice )
        {
            offersMatchingPrice = null;
            List<Myorder> allegroEntries = model.parameters.myorders.myorders
                .Where(x => 
                   Convert.ToDecimal(x.payment.buyerPaidAmount.amount) == Convert.ToDecimal(entry.Amount.ToString().Trim('-'))
                ).ToList();

            if (allegroEntries.Count == 0)
            {
                allegroEntries = model.parameters.myorders.myorders
                    .Where(x => 
                        x.offers.Any(x=>Convert.ToDecimal(x.offerPrice.amount)  == Convert.ToDecimal(entry.Amount.ToString().Trim('-')))
                    ).ToList();

            }

            if (allegroEntries.Count == 0)
            {
                return null;
            }
            if (allegroEntries.Count == 1)
            {
                return allegroEntries;
            }
            else
            {
                var dateFilteredEntries = allegroEntries.Where(x => x.payment.endDate.Date == entry.Date).ToList();
                if (dateFilteredEntries.Count < 1)
                {
                    if (entry.Amount > 0)
                    {
                        //if it's a refund, then we have one more chance of finding the right one
                        //the refund must have happened AFTER the purchase
                        dateFilteredEntries = allegroEntries.Where(x => x.payment.endDate.Date < entry.Date).ToList();

                        if (dateFilteredEntries.Count != 1)
                        {
                            //so now we have a potentially long list of entries purchased at the same price (e.g. 9.99),
                            //so we cannot figure out which one was actually refunded.
                            //too bad there is no refund note or reference
                             offersMatchingPrice = dateFilteredEntries
                                 .SelectMany(x=> x.offers.Where(x=>Convert.ToDecimal(x.offerPrice.amount)  == Convert.ToDecimal(entry.Amount.ToString().Trim('-')))
                                ).ToList();
                            
                            return null;
                        }
                        else
                        {
                            return dateFilteredEntries;
                        }
                    }
                    
                    
                    Console.WriteLine($"ERROR - TOO FEW ENTRIES WHEN RECOGNIZING ALLEGRO ENTRY FOR {entry.Note}");
                }

                return dateFilteredEntries;
            }
        }
    }
}