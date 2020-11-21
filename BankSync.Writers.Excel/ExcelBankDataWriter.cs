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
            sheet.FreezeTopRow();
            
            sheet.Cells[0, 0] = "Entry ID";
            sheet.Cells[0, 1] = "Account";
            sheet.ColumnWidths[1] = 12;

            sheet.Cells[0, 2] = "Date";
            sheet.ColumnWidths[2] = 13;
            sheet.Cells[0, 3] = "Currency";
            sheet.Cells[0, 4] = "Amount";
            sheet.Cells[0, 5] = "Balance";
            sheet.Cells[0, 6] = "PaymentType";
            sheet.ColumnWidths[6] = 25;
            sheet.Cells[0, 7] = "Recipient";
            sheet.ColumnWidths[7] = 35;

            sheet.Cells[0, 8] = "Payer";
            sheet.ColumnWidths[8] = 20;

            sheet.Cells[0, 9] = "Note";
            sheet.ColumnWidths[9] = 55;

            sheet.Cells[0, 10] = "Category";
            sheet.ColumnWidths[10] = 25;

            sheet.Cells[0, 11] = "Subcategory";
            sheet.ColumnWidths[11] = 25;

            sheet.Cells[0, 12] = "Tags";

            sheet.Cells[0, 13] = "Full Details";
            sheet.ColumnWidths[13] = 70;

            for (int i = 0; i < sheet.Cells.ColumnCount; i++)
            {
                sheet.Cells[0, i].Bold = true;
                sheet.Cells[0, i].Fill.BackgroundColor = Color.Gray;
            }

            for (int rowIndex = 1; rowIndex < data.Entries.Count; rowIndex++)
            {
                BankEntry bankEntry = data.Entries[rowIndex];
                sheet.Cells[rowIndex, 0] = bankEntry.OriginalBankEntryId; 
                if (rowIndex % 2 == 0)
                {
                    for (int columnIndex = 0; columnIndex < sheet.Cells.ColumnCount; columnIndex++)
                    {
                        if (columnIndex == 10 || columnIndex == 11)
                        {
                            continue;
                        }
                        sheet.Cells[rowIndex, columnIndex].Fill.BackgroundColor = Color.WhiteSmoke;
                    }
                }
                
                sheet.Cells[rowIndex, 1] = bankEntry.Account;
                sheet.Cells[rowIndex, 2] = new Cell(CellType.Date, bankEntry.Date, "dd/MM/yyyy"); 
                sheet.Cells[rowIndex, 3] = bankEntry.Currency;
                sheet.Cells[rowIndex, 4] = bankEntry.Amount;
                if (bankEntry.Amount < 0)
                {
                    sheet.Cells[rowIndex, 4].TextColor = Color.Red;
                    sheet.Cells[rowIndex, 4].Bold = true;
                }
                else
                {
                    sheet.Cells[rowIndex, 4].TextColor = Color.Green;
                    sheet.Cells[rowIndex, 4].Bold = true;

                }
                sheet.Cells[rowIndex, 5] = bankEntry.Balance;
                sheet.Cells[rowIndex, 6] = bankEntry.PaymentType;
                sheet.Cells[rowIndex, 7] = bankEntry.Recipient;
                sheet.Cells[rowIndex, 7].Border = CellBorder.Right | CellBorder.Left;
                sheet.Cells[rowIndex, 7].Bold = true;
                
                sheet.Cells[rowIndex, 8] = bankEntry.Payer;
                sheet.Cells[rowIndex, 9] = bankEntry.Note;
                sheet.Cells[rowIndex, 10] = bankEntry.Category;
                sheet.Cells[rowIndex, 10].Fill.BackgroundColor = Color.MediumSeaGreen;
                sheet.Cells[rowIndex, 11] = bankEntry.Subcategory;
                sheet.Cells[rowIndex, 11].Fill.BackgroundColor = Color.DarkSeaGreen;

                sheet.Cells[rowIndex, 12] = string.Join(";", bankEntry.Tags);
                sheet.Cells[rowIndex, 13] = bankEntry.FullDetails;

                

            }

            Workbook workbook = new Workbook();
            workbook.Add(sheet);
            workbook.Save(this.targetFilePath);
        }
    }
}
