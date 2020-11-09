using System;
using System.Xml.Linq;
using BankSync.Exporters.Ipko.Mappers;
using BankSync.Model;
using Microsoft.Win32.SafeHandles;

namespace BankSync.Exporters.Ipko.DataTransformation
{
    public class IpkoDataTransformer
    {
        private readonly IDataMapper mapper;
        private readonly DescriptionDataExtractor descriptionDataExtractor;

        public IpkoDataTransformer(IDataMapper mapper)
        {
            this.mapper = mapper;
            this.descriptionDataExtractor = new DescriptionDataExtractor();
        }

        public WalletDataSheet Transform(XDocument xDocument)
        {
            var sheet = new WalletDataSheet();
            foreach (XElement operation in xDocument.Descendants("operation"))
            {

                var entry = new WalletEntry()
                {
                    WalletEntryId = operation.Value.GetHashCode(),
                    Date = this.GetDate(operation),
                    Amount = this.GetAmount(operation),
                    Balance = this.GetBalance(operation),
                    Currency = this.GetCurrency(operation),
                    PaymentType =this.mapper.Map(this.GetPaymentType(operation)),
                };

                entry.Payer = this.mapper.Map(this.GetPayer(entry, operation));
                entry.Recipient = this.mapper.Map(this.GetRecipient(entry, operation));
                entry.Note = this.mapper.Map(this.GetNote(entry,operation));


                sheet.Entries.Add(entry);

            }

            return sheet;
        }

        private string GetNote(WalletEntry entry, XElement operation)
        {
            var element = operation.Element("description");
            if (element != null)
            {
                string note = this.descriptionDataExtractor.GetNote(element.Value);
                if (entry.PaymentType == "Prowizja" 
                || entry.PaymentType == "Opłata"
                || entry.PaymentType == "Wypłata z bankomatu"
                )
                {
                    note = $"{entry.PaymentType} - {note}";
                }

                return note;
            }

            return "";
        }

        private string GetRecipient(WalletEntry entry, XElement operation)
        {
            if (entry.PaymentType == "Przelew na rachunek" || entry.PaymentType == "Zwrot w terminalu")
            {
                return "Wspólne konto";
            }
            if (entry.PaymentType == "Wypłata z bankomatu")
            {
                return entry.Payer;
            }
            if (entry.PaymentType == "Prowizja"
                || entry.PaymentType == "Opłata")
            {
                return "Bank";
            }

            var element = operation.Element("description");
            if (element != null)
            {
                return this.descriptionDataExtractor.GetRecipient(element.Value);
            }

            return "";
        }

        private string GetPayer(WalletEntry entry, XElement operation)
        {
            if (entry.PaymentType == "Przelew z rachunku" 
                || entry.PaymentType == "Zlecenie stałe"
                || entry.PaymentType == "Polecenie Zapłaty" 
                || entry.PaymentType == "Prowizja" 
                || entry.PaymentType == "Opłata")
            {
                return "Wspólne konto";
            }
            var element = operation.Element("description");
            if (element != null)
            {
                return this.descriptionDataExtractor.GetPayer(element.Value);
            }
            return "";
        }

        private string GetPaymentType(XElement operation)
        {
            var element = operation.Element("type");
            if (element != null)
            {
                return element.Value;
            }

            return "";
        }

        private DateTime GetDate(XElement operation)
        {
            var element = operation.Element("order-date");
            if (element != null)
            {
                return DateTime.Parse(element.Value);
            }
            return DateTime.MinValue;
            
        }  
        private decimal GetAmount(XElement operation)
        {
            var element = operation.Element("amount");
            if (element != null)
            {
                return Convert.ToDecimal(element.Value);
            }
            return 0;
        }

        private decimal GetBalance(XElement operation)
        {
            var element = operation.Element("ending-balance");
            if (element != null)
            {
                return Convert.ToDecimal(element.Value);
            }
            return 0;
        }

        private string GetCurrency(XElement operation)
        {
            var element = operation.Element("amount");
            if (element != null)
            {
                return element.Attribute("curr")?.Value??"";
            }
            return "";
        }
    }
}