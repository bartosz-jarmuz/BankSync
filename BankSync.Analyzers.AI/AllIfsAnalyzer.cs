using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BankSync.Logging;
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
        private readonly IBankSyncLogger logger;
        private readonly List<CategoryMap> expenseCategories = new List<CategoryMap>();
        private readonly List<CategoryMap> incomeCategories = new List<CategoryMap>();

        public AllIfsAnalyzer(FileInfo dictionaryFile, IBankSyncLogger logger)
        {
            this.logger = logger;
            this.LoadDictionary(dictionaryFile);
        }

        private void LoadDictionary(FileInfo dictionaryFile)
        {
            XDocument xDoc = XDocument.Load(dictionaryFile.FullName);

            foreach (XElement categoryElement in xDoc.Root.Element("Expense").Descendants("Category"))
            {
                CategoryMap category = LoadCategoryMap(categoryElement);

                this.expenseCategories.Add(category);
            }
            
            foreach (XElement categoryElement in xDoc.Root.Element("Income").Descendants("Category"))
            {
                CategoryMap category = LoadCategoryMap(categoryElement);

                this.incomeCategories.Add(category);
            }

        }

        private static CategoryMap LoadCategoryMap(XElement categoryElement)
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

            return category;
        }

        private static List<string> LoadTokensFromElement(XElement categoryElement)
        {
            List<string> allDirectTokens = new List<string>();
            foreach (XElement mappingElement in categoryElement.Elements("MapFrom"))
            {
                var tokenized = mappingElement.Value.Split(";", StringSplitOptions.RemoveEmptyEntries).Where(x => !string.IsNullOrWhiteSpace(x));
                allDirectTokens.AddRange(tokenized);
            }

            return allDirectTokens.Distinct().ToList();
        }

        public void AssignCategories(BankDataSheet data)
        {
            foreach (BankEntry bankEntry in data.Entries)
            {
                if (bankEntry.Amount < 0)
                {
                    this.AssignExpenseCategories(bankEntry);
                }
                else
                {
                    this.AssignIncomeCategories(bankEntry);

                }
            }
            this.AddCategoryList(data);
            
        }

        private void AssignExpenseCategories(BankEntry bankEntry)
        {
            bool isAssigned = false;
            foreach (CategoryMap category in this.expenseCategories)
            {
                foreach (SubcategoryMap subcategory in category.Subcategories)
                {
                    foreach (string keyword in subcategory.MapFrom)
                    {
                        try
                        {
                            if (bankEntry.Recipient.ContainsNationalUnaware(keyword)
                                || bankEntry.PaymentType.ContainsNationalUnaware(keyword)
                                || bankEntry.Note.ContainsNationalUnaware(keyword))
                            {
                                bankEntry.Subcategory = subcategory.Name;
                                bankEntry.Category = category.Name;
                                isAssigned = true;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.Debug(e.ToString());
                        }
                    }
                }

                if (!isAssigned)
                {
                    foreach (string keyword in category.MapFrom)
                    {
                        try
                        {
                            if (bankEntry.Recipient.ContainsNationalUnaware(keyword)
                                || bankEntry.PaymentType.ContainsNationalUnaware(keyword)
                                || bankEntry.Note.ContainsNationalUnaware(keyword))
                            {
                                bankEntry.Category = category.Name;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.Debug(e.ToString());
                        }
                    }
                }
            }
        }
        
        private void AssignIncomeCategories(BankEntry bankEntry)
        {
            bool isAssigned = false;
            foreach (CategoryMap category in this.incomeCategories)
            {
                foreach (SubcategoryMap subcategory in category.Subcategories)
                {
                    foreach (string keyword in subcategory.MapFrom)
                    {
                        try
                        {
                            if (bankEntry.Payer.ContainsNationalUnaware(keyword)
                                || bankEntry.PaymentType.ContainsNationalUnaware(keyword)
                                || bankEntry.Note.ContainsNationalUnaware(keyword))
                            {
                                bankEntry.Subcategory = subcategory.Name;
                                bankEntry.Category = category.Name;
                                isAssigned = true;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.Debug(e.ToString());

                        }
                    }
                }

                if (!isAssigned)
                {
                    foreach (string keyword in category.MapFrom)
                    {
                        try
                        {
                            if (bankEntry.Payer.ContainsNationalUnaware(keyword)
                                || bankEntry.PaymentType.ContainsNationalUnaware(keyword)
                                || bankEntry.Note.ContainsNationalUnaware(keyword))
                            {
                                bankEntry.Category = category.Name;
                                break;
                            }
                        }
                        catch (Exception e)
                        {
                            this.logger.Debug(e.ToString());
                        }
                    }
                }
            }
        }


        private void AddCategoryList(BankDataSheet data)
        {
            foreach (CategoryMap category in this.expenseCategories.Concat(this.incomeCategories))
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
