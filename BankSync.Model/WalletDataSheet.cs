using System.Collections.Generic;

namespace BankSync.Model
{
    public class WalletDataSheet
    {
        public List<WalletEntry> Entries { get; set; } = new List<WalletEntry>();
    }
}