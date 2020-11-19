using System.Globalization;
using System.IO;
using BankSync.Model;
using CsvHelper;

namespace BankSync.Writers.Csv
{
    public class CsvBankDataWriter : IBankDataWriter
    {
        private readonly string targetFilePath;

        public CsvBankDataWriter(string targetFilePath)
        {
            this.targetFilePath = targetFilePath;
        }

        public void Write(BankDataSheet data)
        {
            using StreamWriter writer = new StreamWriter(this.targetFilePath);
            using CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csv.WriteRecords(data.Entries);
        }
    }
}
