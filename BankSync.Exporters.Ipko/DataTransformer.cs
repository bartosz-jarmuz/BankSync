using System;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using BankSync.Model;

namespace BankSync.Exporters.Ipko
{
    class DataTransformer
    {
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
                    Category = this.GetCategory(operation),
                    Payer = this.GetPayer(operation),
                };
                sheet.Entries.Add(entry);

            }

            return sheet;
        }

        private string GetPayer(XElement operation)
        {
            var element = operation.Element("description");
            if (element != null)
            {
                
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

    public static class DescriptionDataExtractor
    {
        public static string GetPayer(string description)
        {
            if (description.StartsWith("Numer telefonu: "))
            {
                var part = description.Substring("Numer telefonu: ".Length);
                return part.Remove(part.IndexOf("Lokalizacja")).Trim();
            }

            if (description.Contains("Numer karty: "))
            {
                var part = description.Substring(description.IndexOf("Numer karty: ") + "Numer karty: ".Length);
                return part.Trim();
            }

            if (description.Contains("Nazwa nadawcy: "))
            {
                Regex regex = new Regex("(Nazwa nadawcy: )(.*)");
                var match = regex.Match(description);
                return match.Groups[2].Value.Trim();
            }


            return "";
        }
    }
}