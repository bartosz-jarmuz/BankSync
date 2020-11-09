using System;

namespace BankSync.Model
{
    public class WalletEntry
    {
        public WalletEntry()
        {
        }

        public static WalletEntry Clone(WalletEntry toBeCloned)
        {
            return new WalletEntry()
            {
                Payer = toBeCloned.Payer,
                Recipient = toBeCloned.Recipient,
                PaymentType = toBeCloned.PaymentType,
                Balance = toBeCloned.Balance,
                Date = toBeCloned.Date,
                Currency = toBeCloned.Currency,
                WalletEntryId = toBeCloned.WalletEntryId,
                Amount = toBeCloned.Amount,
                Note = toBeCloned.Note,
            };
        }

        public int WalletEntryId { get; set; }
        public string Payer { get; set; }
        public string Recipient { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string Note { get; set; }
        public string PaymentType { get; set; }
    }
}