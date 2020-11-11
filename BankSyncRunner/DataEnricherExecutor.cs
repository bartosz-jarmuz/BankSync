// -----------------------------------------------------------------------
//  <copyright file="DataEnricherExecutor.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Enrichers.Allegro;
using BankSync.Model;

namespace BankSyncRunner
{
    public class DataEnricherExecutor
    {
        private List<IBankDataEnricher> enrichers = new List<IBankDataEnricher>();

        public void LoadEnrichers()
        {
            var enrichersFile = new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Enrichers.xml");

            XDocument xDoc = XDocument.Load(enrichersFile.FullName);

            foreach (XElement allegroEnricher in xDoc.Descendants("Allegro"))
            {
                var enricher = new AllegroBankDataEnricher(allegroEnricher.Parent.Attribute("To").Value);
                this.enrichers.Add(enricher);
            }

            
        }

        public async Task EnrichData(WalletDataSheet data)
        {
            foreach (IBankDataEnricher bankDataEnricher in this.enrichers)
            {
                await bankDataEnricher.Enrich(data);
            }
        }
    }
}