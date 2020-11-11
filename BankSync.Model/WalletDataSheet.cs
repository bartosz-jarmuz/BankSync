using System.Collections.Generic;
using System.Linq;

namespace BankSync.Model
{
    public class WalletDataSheet
    {
        public static WalletDataSheet Consolidate(IEnumerable<WalletDataSheet> sheets)
        {
            var uniqueEntries = new List<WalletEntry>();
            foreach (WalletDataSheet walletDataSheet in sheets)
            {
                foreach (WalletEntry walletEntry in walletDataSheet.Entries)
                {
                    var id = walletEntry.GetWalletEntryId();
                    if (uniqueEntries.All(x => x.GetWalletEntryId() != id))
                    {
                        uniqueEntries.Add(walletEntry);
                    }
                    else
                    {

                    }
                }
            }
            var consolidated = new WalletDataSheet();
            consolidated.Entries = uniqueEntries.OrderByDescending(x => x.Date).ToList();
            return consolidated;
        }
        public List<WalletEntry> Entries { get; set; } = new List<WalletEntry>();
    }
}