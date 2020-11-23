using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BankSync.Model;
using BankSync.Utilities;

namespace BankSync.Analyzers.AI
{

    internal class SubcategoryMap
    {
        public string Name { get; set; }
        public List<string> MapFrom { get; set; } = new List<string>();
    }

    internal class CategoryMap
    {
        public string Name { get; set; }
        public List<SubcategoryMap> Subcategories { get; set; } = new List<SubcategoryMap>();
        public List<string> MapFrom { get; set; } = new List<string>();
    }

    

    public class AllIfsAnalyzer : IBankDataAnalyzer
    {
        private readonly List<CategoryMap> categories = new List<CategoryMap>();

        public AllIfsAnalyzer(FileInfo dictionaryFile)
        {
            this.LoadDictionary(dictionaryFile);
        }

        private void LoadDictionary(FileInfo dictionaryFile)
        {
            XDocument xDoc = XDocument.Load(dictionaryFile.FullName);

            foreach (XElement categoryElement in xDoc.Root.Descendants("Category"))
            {
                CategoryMap category = new CategoryMap()
                {
                    Name = categoryElement.Attribute("Name").Value
                };


                List<string> allDirectTokens = LoadTokensFromElement(categoryElement);
                category.MapFrom = new List<string>(allDirectTokens);

                foreach (XElement subcategoryElement in categoryElement.Elements("Subcategory"))
                {
                    SubcategoryMap subcategory = new SubcategoryMap();
                    subcategory.Name = subcategoryElement.Attribute("Name").Value;
                    subcategory.MapFrom = LoadTokensFromElement(subcategoryElement);
                    category.Subcategories.Add(subcategory);
                }

                this.categories.Add(category);
            }

        }

        private static List<string> LoadTokensFromElement(XElement categoryElement)
        {
            List<string> allDirectTokens = new List<string>();
            foreach (XElement mappingElement in categoryElement.Elements("MapFrom"))
            {
                string[] tokenized = mappingElement.Value.Split(";", StringSplitOptions.RemoveEmptyEntries);
                allDirectTokens.AddRange(tokenized);
            }

            return allDirectTokens.Distinct().ToList();
        }

        public void AssignCategories(BankDataSheet data)
        {
            foreach (BankEntry bankEntry in data.Entries)
            {
                bool isAssigned = false;
                foreach (CategoryMap category in this.categories)
                {
                    foreach (SubcategoryMap subcategory in category.Subcategories)
                    {
                        foreach (string keyword in subcategory.MapFrom)
                        {
                            if (bankEntry.Recipient.ContainsNationalUnaware(keyword)
                                || bankEntry.Note.ContainsNationalUnaware(keyword))
                            {
                                bankEntry.Subcategory = subcategory.Name;
                                bankEntry.Category = category.Name;
                                isAssigned = true;
                                break;
                            }
                        }
                    }

                    if (!isAssigned)
                    {
                        foreach (string keyword in category.MapFrom)
                        {
                            if (bankEntry.Recipient.ContainsNationalUnaware(keyword)
                                || bankEntry.Note.ContainsNationalUnaware(keyword))
                            {
                                bankEntry.Category = category.Name;
                                break;
                            }
                        }
                    }
                }
            }
            this.AddCategoryList(data);
            
        }


        private void AddCategoryList(BankDataSheet data)
        {
            foreach (CategoryMap category in this.categories)
            {
                Category existingCategory = data.Categories.FirstOrDefault(x => x.Name == category.Name);
                if (existingCategory != null)
                {
                    foreach (SubcategoryMap subcategory in category.Subcategories)
                    {
                        Subcategory existingSubcategory = existingCategory.Subcategories.FirstOrDefault(x => x.Name == subcategory.Name);
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
