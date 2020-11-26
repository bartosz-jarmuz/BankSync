using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BankSync.Model;
using BankSync.Utilities;

namespace BankSync.Analyzers.InternalTransactions
{

    internal class RecipientToPayersMap
    {
        public string Recipient { get; set; }

        public List<string> Payers { get; set; } = new List<string>();

    }
    
   internal class SubcategoryMap
    {
        public string Name { get; set; }
        public List<RecipientToPayersMap> MapFrom { get; set; } = new List<RecipientToPayersMap>();
    }

    internal class CategoryMap
    {
        public string Name { get; set; }
        public List<SubcategoryMap> Subcategories { get; set; } = new List<SubcategoryMap>();
    }



    public class InternalTransactionsAnalyzer : IBankDataAnalyzer
    {
        private readonly List<CategoryMap> categories = new List<CategoryMap>();

        public InternalTransactionsAnalyzer(FileInfo dictionaryFile)
        {
            this.LoadDictionary(dictionaryFile);
        }

        private void LoadDictionary(FileInfo dictionaryFile)
        {
            XDocument xDoc = XDocument.Load(dictionaryFile.FullName);

            foreach (XElement categoryElement in xDoc.Root.Descendants("Category"))
            {
                CategoryMap category = LoadCategoryMap(categoryElement);

                this.categories.Add(category);
            }
        }

        private static CategoryMap LoadCategoryMap(XElement categoryElement)
        {
            CategoryMap category = new CategoryMap()
            {
                Name = categoryElement.Attribute("Name").Value
            };


            foreach (XElement subcategoryElement in categoryElement.Elements("Subcategory"))
            {
                SubcategoryMap subcategory = new SubcategoryMap
                {
                    Name = subcategoryElement.Attribute("Name").Value,
                };

                foreach (XElement mapElement in subcategoryElement.Elements("Map"))
                {
                    var mapping = new RecipientToPayersMap();
                    mapping.Recipient = mapElement.Attribute("Recipient").Value;
                    foreach (var payer in mapElement.Elements("Payer"))
                    {
                        mapping.Payers.Add(payer.Value);
                    }

                    subcategory.MapFrom.Add(mapping);
                }

                category.Subcategories.Add(subcategory);
            }

            return category;
        }


        public void AssignCategories(BankDataSheet data)
        {
            foreach (BankEntry bankEntry in data.Entries)
            {
                this.AssignIncomeCategories(bankEntry);
            }

            this.AddCategoryList(data);

        }

        private void AssignIncomeCategories(BankEntry bankEntry)
        {
            foreach (CategoryMap category in this.categories)
            {
                foreach (SubcategoryMap subcategory in category.Subcategories)
                {
                    foreach (RecipientToPayersMap mapping in subcategory.MapFrom)
                    {
                        if (bankEntry.Recipient == mapping.Recipient)
                        {
                            if (mapping.Payers.Any(payer => bankEntry.Payer == payer))
                            {
                                bankEntry.Subcategory = subcategory.Name;
                                bankEntry.Category = category.Name;
                                return;
                            }
                        }
                    }
                }
            }
        }


        private void AddCategoryList(BankDataSheet data)
        {
            foreach (CategoryMap category in this.categories.Concat(this.categories))
            {
                Category existingCategory = data.Categories.FirstOrDefault(x => x.Name == category.Name);
                if (existingCategory != null)
                {
                    foreach (SubcategoryMap subcategory in category.Subcategories)
                    {
                        Subcategory existingSubcategory =
                            existingCategory.Subcategories.FirstOrDefault(x => x.Name == subcategory.Name);
                        if (existingSubcategory == null)
                        {
                            existingCategory.Subcategories.Add(new Subcategory(subcategory.Name));
                        }
                    }
                }
                else
                {
                    Category newCategory = new Category(category.Name);

                    foreach (SubcategoryMap subcategory in category.Subcategories)
                    {
                        newCategory.Subcategories.Add(new Subcategory(subcategory.Name));
                    }

                    data.Categories.Add(newCategory);
                }
            }
        }
    }
}