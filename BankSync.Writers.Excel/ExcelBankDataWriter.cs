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


        public void Write(BankDataSheet data)
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

            sheet.Cells[0, 10] = "Category";
            sheet.ColumnWidths[10] = 30;

            sheet.Cells[0, 11] = "Subcategory";
            sheet.ColumnWidths[11] = 30;

            sheet.Cells[0, 12] = "Tags";

            sheet.Cells[0, 13] = "Full Details";
            sheet.ColumnWidths[13] = 70;

            for (int index = 0; index < data.Entries.Count; index++)
            {
                BankEntry bankEntry = data.Entries[index];
                sheet.Cells[index+1, 0] = bankEntry.OriginalBankEntryId; 
                sheet.Cells[index+1, 1] = bankEntry.Account;
                sheet.Cells[index+1, 2] = new Cell(CellType.Date, bankEntry.Date, "dd/MM/yyyy"); 
                sheet.Cells[index+1, 3] = bankEntry.Currency;
                sheet.Cells[index+1, 4] = bankEntry.Amount;
                sheet.Cells[index+1, 5] = bankEntry.Balance;
                sheet.Cells[index+1, 6] = bankEntry.PaymentType;
                sheet.Cells[index+1, 7] = bankEntry.Recipient;
                sheet.Cells[index+1, 8] = bankEntry.Payer;
                sheet.Cells[index+1, 9] = bankEntry.Note;
                sheet.Cells[index+1, 10] = bankEntry.Category;
                sheet.Cells[index+1, 11] = bankEntry.Subcategory;
                sheet.Cells[index+1, 12] = string.Join(";", bankEntry.Tags);
                sheet.Cells[index + 1, 13] = bankEntry.FullDetails;
            }

            Workbook workbook = new Workbook();
            workbook.Add(sheet);
            workbook.Save(this.targetFilePath);
        }
    }
}
