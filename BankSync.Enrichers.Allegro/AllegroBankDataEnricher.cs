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

            for (int index = 0; index < data.Entries.Count; index++)
            {
                BankEntry entry = data.Entries[index];

                if (IsAllegro(entry))
                {
                    List<Myorder> allegroEntries = this.GetRelevantEntry(entry, allData);
                    if (allegroEntries != null && allegroEntries.Any())
                    {
                        foreach (Myorder allegroEntry in allegroEntries)
                        {
                            foreach (Offer offer in allegroEntry.offers)
                            {
                                BankEntry newEntry = BankEntry.Clone(entry);
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

        private static bool IsAllegro(BankEntry entry)
        {
            if (entry.Note.Contains("00000050668427903"))
            {
                
            }
            var value = entry.Recipient.Contains("allegro.pl", StringComparison.OrdinalIgnoreCase) || entry.Recipient.Contains("PAYU*ALLEGRO", StringComparison.OrdinalIgnoreCase);
            if (value)
            {
                return true;
            }

            value = entry.Note?.Contains("allegro", StringComparison.OrdinalIgnoreCase)??false;
            
            return value;
        }

        private List<Myorder> GetRelevantEntry(BankEntry entry, List<AllegroDataContainer> allegroDataContainers)
        {
            AllegroData model = allegroDataContainers.FirstOrDefault(x => x.ServiceUserName == entry.Payer)?.Model;
            List<Myorder> result = null;
            if (model != null)
            {
                result = GetAllegroEntries(entry, model);
            }

            if (result != null)
            {
                return result;
            }
            else
            {

                //the payer can be empty or it can be somehow incorrect, but if we have an entry that matches the exact price and date... it's probably IT
                foreach (var container in allegroDataContainers)
                {
                    var entries = GetAllegroEntries(entry, container.Model);
                    if (entries != null && entries.Any())
                    {
                        return entries;
                    }
                }
            }


            return null;
        }

        private static List<Myorder> GetAllegroEntries(BankEntry entry, AllegroData model)
        {
            List<Myorder> allegroEntries = model.parameters.myorders.myorders
                .Where(x => x.payment.buyerPaidAmount.amount == entry.Amount.ToString().Trim('-')).ToList();
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