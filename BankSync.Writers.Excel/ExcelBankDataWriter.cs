using BankSync.Model;
using Simplexcel;

namespace BankSync.Writers.Excel
{
    public class ExcelBankDataWriter : IBankDataWriter
    {
        private readonly string targetFilePath;

        public ExcelBankDataWriter(string targetFilePath)
        {
            this.targetFilePath = targetFilePath;
        }


        public void Write(WalletDataSheet data)
        {
            Worksheet sheet = new Worksheet("Data");
            sheet.Cells[0, 0] = "Entry ID";
            sheet.Cells[0, 1] = "Account";
            sheet.Cells[0, 2] = "Date";
            sheet.Cells[0, 3] = "Currency";
            sheet.Cells[0, 4] = "Amount";
            sheet.Cells[0, 5] = "Balance";
            sheet.Cells[0, 6] = "PaymentType";
            sheet.ColumnWidths[6] = 30;

            sheet.Cells[0, 7] = "Recipient";
            sheet.ColumnWidths[7] = 20;

            sheet.Cells[0, 8] = "Payer";
            sheet.ColumnWidths[8] = 20;

            sheet.Cells[0, 9] = "Note";
            sheet.ColumnWidths[9] = 50;

            sheet.Cells[0, 10] = "Tags";
            sheet.ColumnWidths[10] = 70;

            sheet.Cells[0, 11] = "Full Details";
            sheet.ColumnWidths[11] = 70;

            for (int index = 0; index < data.Entries.Count; index++)
            {
                WalletEntry walletEntry = data.Entries[index];
                sheet.Cells[index+1, 0] = walletEntry.WalletEntryId; 
                sheet.Cells[index+1, 1] = walletEntry.Account;
                sheet.Cells[index+1, 2] = new Cell(CellType.Date, walletEntry.Date, "dd/MM/yyyy"); 
                sheet.Cells[index+1, 3] = walletEntry.Currency;
                sheet.Cells[index+1, 4] = walletEntry.Amount;
                sheet.Cells[index+1, 5] = walletEntry.Balance;
                sheet.Cells[index+1, 6] = walletEntry.PaymentType;
                sheet.Cells[index+1, 7] = walletEntry.Recipient;
                sheet.Cells[index+1, 8] = walletEntry.Payer;
                sheet.Cells[index+1, 9] = walletEntry.Note;
                sheet.Cells[index+1, 10] = string.Join(";", walletEntry.Tags);
                sheet.Cells[index + 1, 11] = walletEntry.FullDetails;
            }

            Workbook workbook = new Workbook();
            workbook.Add(sheet);
            workbook.Save(this.targetFilePath);
        }
    }
}
