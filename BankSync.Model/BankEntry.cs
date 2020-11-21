using System;
using System.Collections.Generic;
using System.Linq;

namespace BankSync.Model
{
    public class BankEntry
    {
        public BankEntry()
        {
        }

        public static BankEntry Clone(BankEntry toBeCloned)
        {
            return new BankEntry()
            {
                Account = toBeCloned.Account,
                Payer = toBeCloned.Payer,
                Recipient = toBeCloned.Recipient,
                PaymentType = toBeCloned.PaymentType,
                Balance = toBeCloned.Balance,
                Date = toBeCloned.Date,
                Currency = toBeCloned.Currency,
                Amount = toBeCloned.Amount,
                Note = toBeCloned.Note,
                Category = toBeCloned.Category,
                Subcategory = toBeCloned.Subcategory,
                FullDetails = toBeCloned.FullDetails,
                Tags = toBeCloned.Tags
            };
        }

        

        public string Payer { get; set; }
        public string Recipient { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string Note { get; set; }
        public string PaymentType { get; set; }
        public string Category { get; set; }
        public string Subcategory { get; set; }
        public List<string> Tags{ get; set; } = new List<string>();

        public string FullDetails{ get; set; }
        public string Account{ get; set; }

        public void AssignTag(string tag)
        {
            if (!this.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                this.Tags.Insert(0, tag);
            }
        }

        /// <summary>
        /// A hash code uniquely identifying an entry as it stands in the bank
        /// It might be different from a 'Wallet' entry, if our entry is split into several by an enricher
        /// </summary>
        public int OriginalBankEntryId
        {
            get
            {
                var input = $"{this.Account}{this.Date:O}{this.Payer}{this.Recipient}{this.Amount}{this.FullDetails}";
                return GetStableHashCode(input);
            }
        }
        
        private static int GetStableHashCode(string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for(int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i+1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i+1];
                }

                return hash1 + (hash2*1566083941);
            }
        }
    }
}