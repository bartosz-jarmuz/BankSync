using System.Collections.Generic;
using System.Linq;

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
                    int id = bankEntry.OriginalBankEntryId;
                    if (uniqueEntries.All(x => x.OriginalBankEntryId != id))
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

        public TagMap TagMap { get; set; }
    }

    public class TagMap
    {
        public List<KeyValuePair<int, List<string>>> Values { get; set; } = new List<KeyValuePair<int, List<string>>>();
    }
}