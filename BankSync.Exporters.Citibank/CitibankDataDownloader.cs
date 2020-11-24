using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BankSync.Config;
using BankSync.Model;

namespace BankSync.Exporters.Citibank
{
    
    
    public class CitibankDataDownloader : IBankDataExporter
    {
        public CitibankDataDownloader(ServiceUser serviceUserConfig, IDataMapper mapper)
        {
            this.oldDataManager = new OldDataManager(serviceUserConfig,new CitibankXmlDataTransformer(mapper), mapper);
        }

        private readonly OldDataManager oldDataManager;

        /// <summary>
        /// This is not a fully ready downloader - more of a mock
        /// It only takes local data and only supports Credit Card XML format
        /// </summary>
        public async Task<BankDataSheet> GetData(DateTime startTime, DateTime endTime)
        {
            
            await Task.Delay(0);
            
            List<BankDataSheet> datasets = new List<BankDataSheet>();
            BankDataSheet oldData = this.oldDataManager.GetOldData();
            datasets.Add(oldData);

            return BankDataSheet.Consolidate(datasets);
         }
    }
}