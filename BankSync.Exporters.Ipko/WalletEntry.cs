using System;

namespace BankSync.Exporters.Ipko
{
    public class WalletEntry
    {
        public string Payer { get; set; }
        public string Recipient { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Note { get; set; }
        public string Category { get; set; }
    }
}