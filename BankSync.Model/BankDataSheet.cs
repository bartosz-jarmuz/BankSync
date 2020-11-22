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

        public static List<BankDataSheet> SplitPerMonth(BankDataSheet sheet)
        {
            var list = new List<BankDataSheet>();
            IEnumerable<IGrouping<string, BankEntry>> groupings = sheet.Entries.GroupBy(x => x.Date.ToString("yyyy-MM"));

            foreach (IGrouping<string, BankEntry> grouping in groupings)
            {
                BankDataSheet clone = sheet.Clone();
                clone.Entries= grouping.ToList();
                list.Add(clone);
            }

            return list;
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
        public BankDataSheet Clone()
        {
            var serialized = JsonConvert.SerializeObject(this);

            return JsonConvert.DeserializeObject<BankDataSheet>(serialized);
        }
    }

 
}