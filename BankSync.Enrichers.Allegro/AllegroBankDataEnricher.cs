// -----------------------------------------------------------------------
//  <copyright file="AllegroBankDataEnricher.cs" >
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
using BankSync.Enrichers.Allegro.Model;
using BankSync.Model;
using Newtonsoft.Json;

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
            List<AllegroDataContainer> allegroData = new List<AllegroDataContainer>();

            foreach (ServiceUser serviceUser in this.config.Users)
            {
                OldDataManager oldDataManager = new OldDataManager(serviceUser);
                oldDataManager.GetOldData();

                AllegroDataContainer container = await new AllegroDataDownloader(serviceUser).GetData(oldestEntry);
                allegroData.Add(container);

                oldDataManager.StoreData(container);
            }

            return allegroData;
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
            return (entry.Recipient.Contains("allegro.pl", StringComparison.OrdinalIgnoreCase) || entry.Recipient.Contains("PAYU*ALLEGRO", StringComparison.OrdinalIgnoreCase));
        }

        private List<Myorder> GetRelevantEntry(BankEntry entry, List<AllegroDataContainer> allegroDataContainers)
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

    internal class OldDataManager
    {
        private ServiceUser serviceUserConfig;
        private DirectoryInfo dataRetentionDirectory;

        public OldDataManager(ServiceUser serviceUserConfig)
        {
            this.serviceUserConfig = serviceUserConfig;
            this.dataRetentionDirectory = this.GetDataRetentionDirectory();
        }


        public BankDataSheet GetOldData()
        {
            List<BankDataSheet> sheets = new List<BankDataSheet>();
            if (this.dataRetentionDirectory != null)
            {
                this.LoadOldDataFromXml(sheets);
            }

            return BankDataSheet.Consolidate(sheets);
        }

        public void StoreData(AllegroDataContainer allegroDataContainer)
        {
            if (this.dataRetentionDirectory != null)
            {
                string path = Path.Combine(this.dataRetentionDirectory.FullName, $"{allegroDataContainer.ServiceUserName}_{allegroDataContainer.OldestEntry:yyyy-MM-dd}_{allegroDataContainer.NewestEntry:yyyy-MM-dd}.json");
                string serialized = JsonConvert.SerializeObject(allegroDataContainer);
                File.WriteAllText(path, serialized);
                
            }
        }

        private void LoadOldDataFromXml(List<BankDataSheet> sheets)
        {
            foreach (FileInfo fileInfo in this.dataRetentionDirectory.GetFiles("*.json"))
            {
            }
        }

        private DirectoryInfo GetDataRetentionDirectory()
        {
            XElement dataRetentionElement = this.serviceUserConfig.UserElement.Element("DataRetentionFolder");
            if (dataRetentionElement != null)
            {
                string pathInConfig = dataRetentionElement.Attribute("Path").Value;
                DirectoryInfo dataDirectory;
                if (Path.IsPathFullyQualified(pathInConfig))
                {
                    dataDirectory = new DirectoryInfo(pathInConfig);
                }
                else
                {
                    dataDirectory = new DirectoryInfo(
                        Path.Combine(
                            Path.GetDirectoryName(this.serviceUserConfig.ServiceConfig.Config.ConfigFilePath), pathInConfig.TrimStart(new[] { '/' })));
                }

                Directory.CreateDirectory(dataDirectory.FullName);

                return dataDirectory;
            }

            return null;
        }
    }
}