using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BankSync.Model;

namespace BankSync.Analyzers.AI
{
    public class AllIfsAnalyzer : IBankDataAnalyzer
    {
        private readonly IDictionary<string, List<string>> lowLevelTags = new Dictionary<string, List<string>>();
        private IDictionary<string, List<string>> highLevelTags;

        public AllIfsAnalyzer(FileInfo dictionaryFile)
        {
            this.LoadDictionary(dictionaryFile);
        }

        private void LoadDictionary(FileInfo dictionaryFile)
        {
            XDocument xDoc = XDocument.Load(dictionaryFile.FullName);

            foreach (XElement fromTag in xDoc.Root.Element("LowLevel").Descendants("From"))
            {
                string[] tokenized = fromTag.Value.Split(";", StringSplitOptions.RemoveEmptyEntries);
                string tag = fromTag.Parent.Attribute("To").Value;
                if (this.lowLevelTags.ContainsKey(tag))
                {
                    foreach (string keyword in tokenized)
                    {
                        if (!this.lowLevelTags[tag].Contains(keyword))
                        {
                            this.lowLevelTags[tag].Add(keyword);
                        }
                    }
                }
                else
                {
                    this.lowLevelTags.Add(tag, tokenized.ToList());
                }
            }

        }

        public void AddTags(WalletDataSheet data)
        {
            foreach (WalletEntry walletEntry in data.Entries)
            {
                foreach (KeyValuePair<string, List<string>> tag in this.lowLevelTags)
                {
                    foreach (string keyword in tag.Value)
                    {
                        if (walletEntry.Recipient.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            || walletEntry.Note.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                        {
                           walletEntry.AssignTag(tag.Key);
                        }
                    }
                }

            }
        }
    }
}
