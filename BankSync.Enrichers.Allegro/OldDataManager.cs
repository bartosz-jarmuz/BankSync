// -----------------------------------------------------------------------
//  <copyright file="OldDataManager.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BankSync.Config;
using Newtonsoft.Json;

namespace BankSync.Enrichers.Allegro
{
    internal class OldDataManager
    {
        private readonly ServiceUser serviceUserConfig;
        private readonly DirectoryInfo dataRetentionDirectory;

        public OldDataManager(ServiceUser serviceUserConfig)
        {
            this.serviceUserConfig = serviceUserConfig;
            this.dataRetentionDirectory = this.GetDataRetentionDirectory();
        }


        public AllegroDataContainer GetOldData()
        {
            List<AllegroDataContainer> sheets = new List<AllegroDataContainer>();
            if (this.dataRetentionDirectory != null)
            {
                foreach (FileInfo fileInfo in this.dataRetentionDirectory.GetFiles("*.json"))
                {
                    sheets.Add(JsonConvert.DeserializeObject<AllegroDataContainer>(File.ReadAllText(fileInfo.FullName)));
                }
            }

            if (sheets.Any())
            {
                return AllegroDataContainer.Consolidate(sheets);
            }

            return null;
        }

        public void StoreData(AllegroDataContainer container)
        {
            if (this.dataRetentionDirectory != null)
            {
                List<AllegroDataContainer> allegroDataContainers = AllegroDataContainer.SplitPerMonth(container);
                foreach (AllegroDataContainer allegroDataContainer in allegroDataContainers)
                {
                    string path = Path.Combine(this.dataRetentionDirectory.FullName,
                        $"{allegroDataContainer.ServiceUserName}_{allegroDataContainer.OldestEntry:yyyy-MM-dd}_{allegroDataContainer.NewestEntry:yyyy-MM-dd}.json");
                    string serialized = JsonConvert.SerializeObject(allegroDataContainer);
                    File.WriteAllText(path, serialized);
                }
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