﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BankSync.Model;

namespace BankSync.Analyzers.AI
{

    public class Subcategory
    {
        public string Name { get; set; }
        public List<string> MapFrom { get; set; } = new List<string>();
    }

    public class Category
    {
        public string Name { get; set; }
        public List<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
        public List<string> MapFrom { get; set; } = new List<string>();
    }

    

    public class AllIfsAnalyzer : IBankDataAnalyzer
    {
        private readonly List<Category> categories = new List<Category>();

        public AllIfsAnalyzer(FileInfo dictionaryFile)
        {
            this.LoadDictionary(dictionaryFile);
        }

        private void LoadDictionary(FileInfo dictionaryFile)
        {
            XDocument xDoc = XDocument.Load(dictionaryFile.FullName);

            foreach (XElement categoryElement in xDoc.Root.Descendants("Category"))
            {
                var category = new Category()
                {
                    Name = categoryElement.Attribute("Name").Value
                };


                List<string> allDirectTokens = LoadTokensFromElement(categoryElement);
                category.MapFrom = new List<string>(allDirectTokens);

                foreach (XElement subcategoryElement in categoryElement.Elements("Subcategory"))
                {
                    var subcategory = new Subcategory();
                    subcategory.Name = subcategoryElement.Attribute("Name").Value;
                    subcategory.MapFrom = LoadTokensFromElement(subcategoryElement);
                    category.Subcategories.Add(subcategory);
                }

                this.categories.Add(category);
            }

        }

        private static List<string> LoadTokensFromElement(XElement categoryElement)
        {
            var allDirectTokens = new List<string>();
            foreach (XElement mappingElement in categoryElement.Elements("MapFrom"))
            {
                string[] tokenized = mappingElement.Value.Split(";", StringSplitOptions.RemoveEmptyEntries);
                allDirectTokens.AddRange(tokenized);
            }

            return allDirectTokens.Distinct().ToList();
        }

        public void AssignCategories(WalletDataSheet data)
        {
            foreach (WalletEntry walletEntry in data.Entries)
            {
                bool isAssigned = false;
                foreach (Category category in this.categories)
                {
                    foreach (Subcategory subcategory in category.Subcategories)
                    {
                        foreach (string keyword in subcategory.MapFrom)
                        {
                            if (walletEntry.Recipient.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                || walletEntry.Note.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                            {
                                walletEntry.Subcategory = subcategory.Name;
                                walletEntry.Category = category.Name;
                                isAssigned = true;
                                break;
                            }
                        }
                    }

                    if (!isAssigned)
                    {
                        foreach (string keyword in category.MapFrom)
                        {
                            if (walletEntry.Recipient.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                || walletEntry.Note.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                            {
                                walletEntry.Category = category.Name;
                                break;
                            }
                        }
                    }

                }
            }
        }
    }
}