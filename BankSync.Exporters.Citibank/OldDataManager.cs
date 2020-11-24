// -----------------------------------------------------------------------
//  <copyright file="OldDataManager.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using BankSync.Config;
using BankSync.Model;

namespace BankSync.Exporters.Citibank
{
    internal class OldDataManager
    {
        private readonly DirectoryInfo dataRetentionDirectory;
        private readonly ServiceUser serviceUserConfig;
        private readonly CitibankXmlDataTransformer xmlTransformer;

        public OldDataManager(ServiceUser serviceUserConfig, CitibankXmlDataTransformer xmlTransformer, IDataMapper mapper)
        {
            this.serviceUserConfig = serviceUserConfig;
            this.xmlTransformer = xmlTransformer;
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


        private void LoadOldDataFromXml(List<BankDataSheet> sheets)
        {
            var accountName = this.serviceUserConfig.UserElement.Attribute("AccountName")?.Value;
            foreach (FileInfo fileInfo in this.dataRetentionDirectory.GetFiles("*.xml"))
            {
                XDocument doc = XDocument.Load(fileInfo.FullName);
                sheets.Add(this.xmlTransformer.TransformXml(doc, accountName));
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