// -----------------------------------------------------------------------
//  <copyright file="OldDataManager.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using BankSync.Config;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Exporters.Ipko.Mappers;
using BankSync.Model;

namespace BankSync.Exporters.Ipko
{
    public class OldDataManager
    {
        private readonly DirectoryInfo dataRetentionDirectory;
        private readonly ServiceUser serviceUserConfig;
        private readonly IpkoXmlDataTransformer xmlTransformer;
        private readonly IpkoTsvDataTransformer tsvTransformer;

        public OldDataManager(ServiceUser serviceUserConfig, IpkoXmlDataTransformer xmlTransformer, IDataMapper mapper)
        {
            this.serviceUserConfig = serviceUserConfig;
            this.xmlTransformer = xmlTransformer;
            this.tsvTransformer = new IpkoTsvDataTransformer(mapper);
            this.dataRetentionDirectory = this.GetDataRetentionDirectory();
        }


        public WalletDataSheet GetOldData()
        {
            var sheets = new List<WalletDataSheet>();
            if (this.dataRetentionDirectory != null)
            {
                this.LoadOldDataFromXml(sheets);
                this.LoadOldDataFromTsv(sheets);
            }

            return WalletDataSheet.Consolidate(sheets);
        }

        public void StoreData(XDocument document, string account, in DateTime startDate, in DateTime endDate)
        {
            if (this.dataRetentionDirectory != null)
            {
                var path = Path.Combine(this.dataRetentionDirectory.FullName,
                    $"{account.Substring(account.Length - 4)}_{startDate:yyyy-MM-dd}_{endDate:yyyy-MM-dd}.xml");
                document.Save(path);
            }
        }

        private void LoadOldDataFromXml(List<WalletDataSheet> sheets)
        {
            foreach (FileInfo fileInfo in this.dataRetentionDirectory.GetFiles("*.xml"))
            {
                XDocument doc = XDocument.Load(fileInfo.FullName);
                sheets.Add(this.xmlTransformer.TransformXml(doc));
            }
        }

        private void LoadOldDataFromTsv(List<WalletDataSheet> sheets)
        {
            foreach (FileInfo fileInfo in this.dataRetentionDirectory.GetFiles("*.tsv"))
            {
                sheets.Add(this.tsvTransformer.TransformTsv(fileInfo));


            }
        }


        private DirectoryInfo GetDataRetentionDirectory()
        {
            var dataRetentionElement = this.serviceUserConfig.UserElement.Element("DataRetentionFolder");
            if (dataRetentionElement != null)
            {
                var pathInConfig = dataRetentionElement.Attribute("Path").Value;
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