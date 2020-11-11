using System;
using System.Reflection;
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

            sheet.Cells[0, 1] = "Date";
            sheet.ColumnWidths[0] = 25;

            sheet.Cells[0, 2] = "Currency";
            sheet.Cells[0, 3] = "Amount";
            sheet.Cells[0, 4] = "Balance";

            sheet.Cells[0, 5] = "PaymentType";
            sheet.ColumnWidths[5] = 30;

            sheet.Cells[0, 6] = "Recipient";
            sheet.ColumnWidths[6] = 20;

            sheet.Cells[0, 7] = "Payer";
            sheet.ColumnWidths[7] = 20;

            sheet.Cells[0, 8] = "Note";
            sheet.ColumnWidths[8] = 50;

            sheet.Cells[0, 9] = "Tags";
            sheet.ColumnWidths[9] = 70;

            for (int index = 0; index < data.Entries.Count; index++)
            {
                WalletEntry walletEntry = data.Entries[index];
                sheet.Cells[index+1, 0] = walletEntry.WalletEntryId; 
                sheet.Cells[index+1, 1] = new Cell(CellType.Date, walletEntry.Date, "dd/MM/yyyy"); 
                sheet.Cells[index+1, 2] = walletEntry.Currency;
                sheet.Cells[index+1, 3] = walletEntry.Amount;
                sheet.Cells[index+1, 4] = walletEntry.Balance;
                sheet.Cells[index+1, 5] = walletEntry.PaymentType;
                sheet.Cells[index+1, 6] = walletEntry.Recipient;
                sheet.Cells[index+1, 7] = walletEntry.Payer;
                sheet.Cells[index+1, 8] = walletEntry.Note;
                sheet.Cells[index+1, 9] = string.Join(";", walletEntry.Tags);
            }

            Workbook workbook = new Workbook();
            workbook.Add(sheet);
            workbook.Save(this.targetFilePath);
        }
    }
}
