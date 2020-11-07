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
                    Date = this.GetDate(operation),
                    Amount = this.GetAmount(operation),
                    Currency = this.GetCurrency(operation),
                    Category =this.mapper.Map(this.GetCategory(operation)),
                    Payer = this.mapper.Map(this.GetPayer(operation)),
                    Recipient = this.mapper.Map(this.GetRecipient(operation)),
                    Note = this.GetNote(operation)
                };
                sheet.Entries.Add(entry);

            }

            return sheet;
        }

        private string GetNote(XElement operation)
        {
            var element = operation.Element("description");
            if (element != null)
            {
                return element.Value;
            }

            return "";
        }

        private string GetRecipient(XElement operation)
        {
            var element = operation.Element("description");
            if (element != null)
            {

            }

            return "";
        }

        private string GetPayer(XElement operation)
        {
            var element = operation.Element("description");
            if (element != null)
            {
                return this.descriptionDataExtractor.GetPayer(element.Value);
            }

            return "";
        }

        private string GetCategory(XElement operation)
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