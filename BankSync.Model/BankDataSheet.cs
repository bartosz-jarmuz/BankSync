using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BankSync.Model
{
    public class BankDataSheet
    {
        public static BankDataSheet Consolidate(IEnumerable<BankDataSheet> sheets)
        {
            List<BankEntry> uniqueEntries = new List<BankEntry>();
            foreach (BankDataSheet bankDataSheeta in sheets)
            {
                foreach (BankEntry bankEntry in bankDataSheeta.Entries)
                {
                    int id = bankEntry.BankEntryId;
                    if (uniqueEntries.All(x => x.BankEntryId != id))
                    {
                        uniqueEntries.Add(bankEntry);
                    }
                }
            }
            BankDataSheet consolidated = new BankDataSheet();
            consolidated.Entries = uniqueEntries.OrderByDescending(x => x.Date).ToList();
            return consolidated;
        }

        public List<BankEntry> Entries { get; set; } = new List<BankEntry>();

        public DateTime GetOldestEntryFor(string account)
        {
            return this.Entries?.LastOrDefault(x => x.Account == account)?.Date ?? default;
        }
        public DateTime GetNewestEntryFor(string account)
        {
            return this.Entries?.FirstOrDefault(x => x.Account == account)?.Date ?? default;
        }

        public void LoadCategories()
        {
            this.Categories = new List<Category>();
            foreach (BankEntry bankEntry in this.Entries)
            {
                if (bankEntry.Category != null)
                {
                    Category existingCategory = this.Categories.FirstOrDefault(x => x.Name == bankEntry.Category);
                    if (existingCategory != null)
                    {
                        if (bankEntry.Subcategory != null)
                        {
                            Subcategory existingSubcategory = existingCategory.Subcategories.FirstOrDefault(x => x.Name == bankEntry.Subcategory);
                            if (existingSubcategory == null)
                            {
                                existingCategory.Subcategories.Add(new Subcategory(bankEntry.Subcategory));
                            }
                        }
                    }
                    else
                    {
                        Category category = new Category(bankEntry.Category);
                        if (bankEntry.Subcategory != null)
                        {
                            category.Subcategories.Add(new Subcategory(bankEntry.Subcategory));
                        }
                        this.Categories.Add(category);
                    }
                }
            }
        }
        
        public List<Category> Categories { get; set; } = new List<Category>();
    }

 
}