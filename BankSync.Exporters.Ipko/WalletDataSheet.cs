using System.Collections.Generic;

namespace BankSync.Exporters.Ipko
{
    public class WalletDataSheet
    {
        public List<WalletEntry> Entries { get; set; } = new List<WalletEntry>();
    }
}