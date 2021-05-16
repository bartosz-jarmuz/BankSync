using System;
using System.Linq;
using System.Xml.Linq;
using BankSync.Model;
using BankSync.Utilities;

namespace BankSync.Exporters.Citibank
{
    internal class CitibankXmlDataTransformer
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo
        private const string PaymentType_splataNależności = "Spłata należności";
        private const string PaymentType_oplataZaKartę = "Opłata za kartę";
        private const string PaymentType_transakcjaKartą = "Transakcja kartą";
        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming
        private readonly IDataMapper mapper;
        private readonly DescriptionDataExtractor descriptionDataExtractor;
        private BankSyncConverter converter;

        public CitibankXmlDataTransformer(IDataMapper mapper)
        {
            this.mapper = mapper;
            this.descriptionDataExtractor = new DescriptionDataExtractor();
            this.converter = new BankSyncConverter();
        }

        public BankDataSheet TransformXml(XDocument xDocument, string accountName)
        {
            BankDataSheet sheet = new BankDataSheet();

            foreach (XElement operation in xDocument.Descendants("Transaction"))
            {
                BankEntry entry = new BankEntry()
                {
                    Account = accountName,
                    Date = this.GetDate(operation),
                    Amount = this.GetAmount(operation),
                    Balance = 0,
                    Currency = "PLN",
                    Note = this.GetDescription(operation)?.Replace("  ", ""),
                    FullDetails = this.GetDescription(operation),
                    PaymentType =this.mapper.Map(this.GetPaymentType(operation)),
                };

                entry.Payer = this.mapper.Map(this.GetPayer(entry, operation));
                entry.Recipient = this.mapper.Map(this.GetRecipient(entry, operation));
                sheet.Entries.Add(entry);
            }

            return sheet;
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

        private string GetRecipient(BankEntry entry, XElement operation)
        {
            if (entry.PaymentType == PaymentType_splataNależności 
                || entry.PaymentType == PaymentType_oplataZaKartę)
            {
                return "Bank";
            }
            
            string recipient = "";
            XElement element = operation.Element("description");
            if (!string.IsNullOrEmpty(element?.Value))
            {
                recipient =  this.descriptionDataExtractor.GetRecipient(element.Value);
            }

            return recipient;
        }

        private string GetPayer(BankEntry entry, XElement operation)
        {
            return this.mapper.Map(entry.Account);
        }

        private string GetPaymentType(XElement operation)
        {
            XElement element = operation.Element("transaction_type");
            if (!string.IsNullOrEmpty(element?.Value))
            {
                if (element.Value.StartsWith("spłata", StringComparison.OrdinalIgnoreCase))
                {
                    return PaymentType_splataNależności;
                }
                if (element.Value.StartsWith("opł.", StringComparison.OrdinalIgnoreCase))
                {
                    return PaymentType_oplataZaKartę;
                }
                
                return PaymentType_transakcjaKartą;

            }

            return PaymentType_transakcjaKartą;
        }

        private DateTime GetDate(XElement operation)
        {
            XElement element = operation.Element("date");
            if (element != null)
            {
                try
                {
                    return DateTime.Parse(element.Value);
                }
                catch (Exception)
                {
                    try
                    {
                        return DateTime.ParseExact(element.Value, "dd/MM/yyyy", null);
                    }
                    catch (Exception)
                    {
                    }
                    //todo handle maybe sometime?
                }
            }
            return DateTime.MinValue;
            
        }  
        private decimal GetAmount(XElement operation)
        {
            XElement element = operation.Element("amount");
            if (element != null)
            {
                return converter.ToDecimal(element.Value);
            }
            return 0;
        }

        
    }
}