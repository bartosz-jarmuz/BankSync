using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.SafeHandles;

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
                Account = toBeCloned.Account,
                Payer = toBeCloned.Payer,
                Recipient = toBeCloned.Recipient,
                PaymentType = toBeCloned.PaymentType,
                Balance = toBeCloned.Balance,
                Date = toBeCloned.Date,
                Currency = toBeCloned.Currency,
                WalletEntryId = toBeCloned.WalletEntryId,
                Amount = toBeCloned.Amount,
                Note = toBeCloned.Note,
                FullDetails = toBeCloned.FullDetails,
                Tags = toBeCloned.Tags
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
        public List<string> Tags{ get; set; } = new List<string>();

        public string FullDetails{ get; set; }
        public string Account{ get; set; }

        public void AssignTag(string tag)
        {
            if (!this.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                this.Tags.Add(tag);
            }
        }
    }
}