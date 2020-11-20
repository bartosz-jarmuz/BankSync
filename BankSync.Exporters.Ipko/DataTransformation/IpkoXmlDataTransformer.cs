using System;
using System.Linq;
using System.Xml.Linq;
using BankSync.Exporters.Ipko.Mappers;
using BankSync.Model;

namespace BankSync.Exporters.Ipko.DataTransformation
{
    public class IpkoXmlDataTransformer
    {
        private readonly IDataMapper mapper;
        private readonly DescriptionDataExtractor descriptionDataExtractor;

        public IpkoXmlDataTransformer(IDataMapper mapper)
        {
            this.mapper = mapper;
            this.descriptionDataExtractor = new DescriptionDataExtractor();
        }

        public BankDataSheet TransformXml(XDocument xDocument)
        {
            BankDataSheet sheet = new BankDataSheet();

            string account = this.GetAccount(xDocument);
            foreach (XElement operation in xDocument.Descendants("operation"))
            {
                BankEntry entry = new BankEntry()
                {
                    Account = account,
                    Date = this.GetDate(operation),
                    Amount = this.GetAmount(operation),
                    Balance = this.GetBalance(operation),
                    Currency = this.GetCurrency(operation),
                    FullDetails = this.GetDescription(operation),
                    PaymentType =this.mapper.Map(this.GetPaymentType(operation)),
                };

                entry.Payer = this.mapper.Map(this.GetPayer(entry, operation));
                entry.Recipient = this.mapper.Map(this.GetRecipient(entry, operation));
                entry.Note = this.mapper.Map(this.GetNote(entry,operation));
                MakeSureRecipientNotEmpty(entry);
                sheet.Entries.Add(entry);
            }

            return sheet;
        }

        private static void MakeSureRecipientNotEmpty(BankEntry entry)
        {
            if (string.IsNullOrEmpty(entry.Recipient) && entry.PaymentType == "Płatność kartą")
            {
                entry.Recipient = entry.Note;
            }
            if (string.IsNullOrEmpty(entry.Recipient) && entry.Amount > 0)
            {
                entry.Recipient = entry.Account;
            }

        }

        private string GetAccount(XDocument xDocument)
        {
            var account = xDocument.Descendants("account").FirstOrDefault();
            if (account == null)
            {
                account = xDocument.Descendants("card").FirstOrDefault();
            }
            return this.mapper.Map(account?.Value)??"Not recognized";
        }

        private string GetDescription(XElement operation)
        {
            XElement element = operation.Element("description");
            if (element != null)
            {
                return element.Value;
            }
            return "";
        }

        private string GetNote(BankEntry entry, XElement operation)
        {
            XElement element = operation.Element("description");
            if (element != null)
            {
                string note = this.descriptionDataExtractor.GetNote(element.Value);
                if (entry.PaymentType == "Prowizja" 
                || entry.PaymentType == "Opłata"
                || entry.PaymentType == "Wypłata z bankomatu"
                || entry.PaymentType == "Wypłata w bankomacie"
                )
                {
                    note = $"{entry.PaymentType} - {note}";
                }

                return note;
            }

            return "";
        }

        private string GetRecipient(BankEntry entry, XElement operation)
        {
            if (entry.PaymentType == "Przelew na rachunek" || entry.PaymentType == "Zwrot w terminalu" || entry.PaymentType == "Spłata należności - Dziękujemy")
            {
                return "Wspólne konto";
            }
            if (entry.PaymentType == "Wypłata z bankomatu" || entry.PaymentType == "Wypłata w bankomacie")
            {
                return entry.Payer;
            }
            if (entry.PaymentType == "Prowizja"
                || entry.PaymentType == "Opłata")
            {
                return "Bank";
            }
            

            XElement element = operation.Element("description");
            if (element != null)
            {
                return this.descriptionDataExtractor.GetRecipient(element.Value);
            }

            return "";
        }

        private string GetPayer(BankEntry entry, XElement operation)
        {
            if (entry.PaymentType == "Płatność kartą")
            {
                return this.mapper.Map(entry.Account);
            }

            if (entry.PaymentType == "Przelew z rachunku" 
                || entry.PaymentType == "Zlecenie stałe"
                || entry.PaymentType == "Polecenie Zapłaty" 
                || entry.PaymentType == "Prowizja" 
                || entry.PaymentType == "Opłata")
            {
                return "Wspólne konto";
            }
            XElement element = operation.Element("description");
            if (element != null)
            {
                return this.descriptionDataExtractor.GetPayer(element.Value);
            }
            return "";
        }

        private string GetPaymentType(XElement operation)
        {
            XElement element = operation.Element("type");
            if (element != null)
            {
                return element.Value;
            }

            return "";
        }

        private DateTime GetDate(XElement operation)
        {
            XElement element = operation.Element("order-date");
            if (element != null)
            {
                return DateTime.Parse(element.Value);
            }
            return DateTime.MinValue;
            
        }  
        private decimal GetAmount(XElement operation)
        {
            XElement element = operation.Element("amount");
            if (element != null)
            {
                return Convert.ToDecimal(element.Value);
            }
            return 0;
        }

        private decimal GetBalance(XElement operation)
        {
            XElement element = operation.Element("ending-balance");
            if (element != null)
            {
                return Convert.ToDecimal(element.Value);
            }
            return 0;
        }

        private string GetCurrency(XElement operation)
        {
            XElement element = operation.Element("amount");
            if (element != null)
            {
                return element.Attribute("curr")?.Value??"";
            }
            return "";
        }
    }
}