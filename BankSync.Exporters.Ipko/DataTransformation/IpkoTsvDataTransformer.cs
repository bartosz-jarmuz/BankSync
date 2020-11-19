using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using BankSync.Exporters.Ipko.Mappers;
using BankSync.Model;

namespace BankSync.Exporters.Ipko.DataTransformation
{
    public class IpkoTsvDataTransformer
    {
        private readonly IDataMapper mapper;

        public IpkoTsvDataTransformer(IDataMapper mapper)
        {
            this.mapper = mapper;
        }

        public BankDataSheet TransformTsv(FileInfo file)
        {
            BankDataSheet sheet = new BankDataSheet();

            string account = this.GetAccount(file);

            string[] lines = File.ReadAllLines(file.FullName);

            foreach (string line in lines)
            {
                string[] data = line.Split('\t', StringSplitOptions.RemoveEmptyEntries);

                BankEntry entry = new BankEntry()
                {
                    Account = account,
                    Date = this.GetDate(data),
                    Amount = this.GetAmount(data),
                    Balance = 0,
                    Currency = this.GetCurrency(data),
                    FullDetails = this.GetDescription(data),
                    PaymentType = this.mapper.Map("Płatność kartą"),
                };

                entry.Payer = this.mapper.Map(this.GetPayer(entry));
                entry.Recipient = this.mapper.Map(this.GetRecipient(data, entry));
                entry.Note = this.mapper.Map(this.GetDescription(data));
                sheet.Entries.Add(entry);
            }

            return sheet;
        }

        private string GetAccount(FileInfo file)
        {
            var name = Path.GetFileNameWithoutExtension(file.Name);
            if (name.IndexOf('_') > 0)
            {
                name = name.Remove(name.IndexOf('_'));
            }

            return this.mapper.Map(name) ?? "Not recognized";
        }

        private string GetDescription(string[] line)
        {
             return line[1];
        }
        private string GetRecipient(string[] line, BankEntry entry)
        {
            if (entry.PaymentType == "Spłata należności - Dziękujemy")
            {
                return "Wspólne konto";
            }
            return line[1] + ", " + line[2];
        }

        private string GetPayer(BankEntry entry)
        {
            if (entry.PaymentType == "Płatność kartą")
            {
                return this.mapper.Map(entry.Account);
            }
            if (entry.PaymentType == "Spłata należności - Dziękujemy")
            {
                return "Wspólne konto";
            }

            return "";
        }

       
        private DateTime GetDate(string[] line)
        {
             return DateTime.Parse(line[0]);
        }  

        private decimal GetAmount(string[] line)
        {
            //card entries in TSV (copied from archive IPKO PDFs) are inverted, i.e. amount spent is positive,
            //and amount paid back to the card is negative
            //NOTE: this is not like that in the XML files
            return Convert.ToDecimal(line[4].Replace(" ","")) * -1;
        }

        private string GetCurrency(string[] line)
        {
            return line[3];
        }
    }
}