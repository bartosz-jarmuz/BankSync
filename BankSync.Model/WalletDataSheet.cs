using System.Collections.Generic;
using System.Linq;

namespace BankSync.Model
{
    public class WalletDataSheet
    {
        public static WalletDataSheet Consolidate(IEnumerable<WalletDataSheet> sheets)
        {
            List<WalletEntry> uniqueEntries = new List<WalletEntry>();
            foreach (WalletDataSheet walletDataSheet in sheets)
            {
                foreach (WalletEntry walletEntry in walletDataSheet.Entries)
                {
                    int id = walletEntry.OriginalBankEntryId;
                    if (uniqueEntries.All(x => x.OriginalBankEntryId != id))
                    {
                        uniqueEntries.Add(walletEntry);
                    }
                }
            }
            WalletDataSheet consolidated = new WalletDataSheet();
            consolidated.Entries = uniqueEntries.OrderByDescending(x => x.Date).ToList();
            return consolidated;
        }
        public List<WalletEntry> Entries { get; set; } = new List<WalletEntry>();

        public TagMap TagMap { get; set; }
    }

    public class TagMap
    {
        public List<KeyValuePair<int, List<string>>> Values { get; set; } = new List<KeyValuePair<int, List<string>>>();
    }
}