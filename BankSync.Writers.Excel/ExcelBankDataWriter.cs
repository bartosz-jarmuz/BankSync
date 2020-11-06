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
            sheet.Cells[0, 0] = "Date";
            sheet.ColumnWidths[0] = 20;

            sheet.Cells[0, 1] = "Currency";
            sheet.Cells[0, 2] = "Amount";
            sheet.Cells[0, 3] = "Payer";
            sheet.ColumnWidths[3] = 20;

            sheet.Cells[0, 4] = "Recipient";
            sheet.ColumnWidths[4] = 20;

            sheet.Cells[0, 5] = "Category";
            sheet.ColumnWidths[5] = 30;

            sheet.Cells[0, 6] = "Note";
            sheet.ColumnWidths[6] = 50;

            for (int index = 0; index < data.Entries.Count; index++)
            {
                WalletEntry walletEntry = data.Entries[index];
                sheet.Cells[index+1, 0] = new Cell(CellType.Date, walletEntry.Date, "dd/MM/yyyy"); 
                sheet.Cells[index+1, 1] = walletEntry.Currency;
                sheet.Cells[index+1, 2] = walletEntry.Amount;
                sheet.Cells[index+1, 3] = walletEntry.Payer;
                sheet.Cells[index+1, 4] = walletEntry.Recipient;
                sheet.Cells[index+1, 5] = walletEntry.Category;
                sheet.Cells[index+1, 6] = walletEntry.Note;
            }

            Workbook workbook = new Workbook();
            workbook.Add(sheet);
            workbook.Save(this.targetFilePath);
        }
    }
}
