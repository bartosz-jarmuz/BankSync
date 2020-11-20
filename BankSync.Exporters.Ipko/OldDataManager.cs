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
using System.Xml.Serialization;
using System.Xml.XPath;
using BankSync.Config;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Exporters.Ipko.Mappers;
using BankSync.Model;
using Newtonsoft.Json;

namespace BankSync.Exporters.Ipko
{
    internal class OldDataManager
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


        public BankDataSheet GetOldData()
        {
            List<BankDataSheet> sheets = new List<BankDataSheet>();
            if (this.dataRetentionDirectory != null)
            {
                this.LoadOldDataFromXml(sheets);
                this.LoadOldDataFromTsv(sheets);
            }

            return BankDataSheet.Consolidate(sheets);
        }

        public void StoreData(XDocument sheet, string account)
        {
            if (this.dataRetentionDirectory != null)
            {
                List<XDocument> partialDocs = this.SplitByMonth(sheet);
                foreach (XDocument partialDoc in partialDocs)
                {
                    IEnumerable<XElement> orderDateElements = partialDoc.XPathSelectElements("//operation/order-date");
                    (DateTime oldest, DateTime newest) dates = this.GetFirstAndLastDates(orderDateElements);
                    string path = Path.Combine(this.dataRetentionDirectory.FullName, $"{account.Substring(account.Length - 4)}_{dates.oldest:yyyy-MM-dd}_{dates.newest:yyyy-MM-dd}.xml");
                    partialDoc.Save(path);
                }
              
            }
        }

        private List<XDocument> SplitByMonth(XDocument sheet)
        {
            List<XDocument> list = new List<XDocument>();
                IEnumerable<XElement> operationElements = sheet.XPathSelectElements("//operation");
                List<IGrouping<string, XElement>> groupings = operationElements.GroupBy(x => Convert.ToDateTime(x.Element("order-date").Value).ToString("yyyy-MM")).ToList();

                foreach (IGrouping<string, XElement> xElements in groupings)
                {
                    (DateTime oldest, DateTime newest) dates = this.GetFirstAndLastDatesFromOperations(xElements);
                    XDocument clone = XDocument.Parse(sheet.ToString());
                    clone.Root.Descendants("operation").Remove();
                    clone.Root.Descendants("operations").First().Add(xElements);

                    XElement dateElement = clone.Root.Element("search").Element("date");
                    dateElement.Attribute("since").Value = dates.oldest.ToString("yyyy-MM-dd");
                    dateElement.Attribute("to").Value = dates.newest.ToString("yyyy-MM-dd");
                    list.Add(clone);
                }

                return list;
        }
        private (DateTime oldest, DateTime newest) GetFirstAndLastDatesFromOperations(IEnumerable<XElement> operationElements)
        {
            List<XElement> orderDateElements = operationElements.Select(x =>x.Element("order-date")).ToList();
            return this.GetFirstAndLastDates(orderDateElements);
        }

        private (DateTime oldest, DateTime newest) GetFirstAndLastDates(IEnumerable<XElement> orderDateElements)
        {
            List<DateTime> dates = orderDateElements.Select(x => Convert.ToDateTime(x.Value)).OrderByDescending(x => x).ToList();
            return (dates.Last(), dates.First());
        }

        private void LoadOldDataFromXml(List<BankDataSheet> sheets)
        {
            foreach (FileInfo fileInfo in this.dataRetentionDirectory.GetFiles("*.xml"))
            {
                XDocument doc = XDocument.Load(fileInfo.FullName);
                sheets.Add(this.xmlTransformer.TransformXml(doc));
            }
        }

        private void LoadOldDataFromTsv(List<BankDataSheet> sheets)
        {
            foreach (FileInfo fileInfo in this.dataRetentionDirectory.GetFiles("*.tsv"))
            {
                sheets.Add(this.tsvTransformer.TransformTsv(fileInfo));


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