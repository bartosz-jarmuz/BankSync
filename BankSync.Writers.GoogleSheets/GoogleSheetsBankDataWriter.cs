using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Model;
using BankSync.Utilities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace BankSync.Writers.GoogleSheets
{
    public class GoogleSheetsBankDataWriter : IBankDataWriter
    {
        private readonly FileInfo googleWriterConfigFile;
        private readonly string readRange = "Data!A:N";
        private readonly string headerRange = "Data!A1:N1";
        private readonly string spreadsheetId;
        private readonly string credentialsPath;
        private readonly int dataSheetId;

        private static readonly string[] Scopes = {SheetsService.Scope.Spreadsheets};
        private static readonly string ApplicationName = "BankSync";
        private int? entryIdRow;
        private int? accountRow;
        private int? dateRow;
        private int? currencyRow;
        private int? amountRow;
        private int? balanceRow;
        private int? paymentTypeRow;
        private int? recipientRow;
        private int? payerRow;
        private int? noteRow;
        private int? categoryRow;
        private int? subcategoryRow;
        private int? tagsRow;
        private int? fullDetailsRow;

        private int?[] allHeadersIndexes => new int?[]
        {
            this.entryIdRow,
            this.accountRow,
            this.dateRow,
            this.currencyRow,
            this.amountRow,
            this.balanceRow,
            this.paymentTypeRow,
            this.recipientRow,
            this.payerRow,
            this.noteRow,
            this.categoryRow,
            this.subcategoryRow,
            this.tagsRow,
            this.fullDetailsRow,
        };
        
        public GoogleSheetsBankDataWriter(FileInfo googleWriterConfigFile)
        {
            this.googleWriterConfigFile = googleWriterConfigFile;
            XDocument xDoc = XDocument.Load(this.googleWriterConfigFile.FullName);
            this.credentialsPath = xDoc.Root.Element("CredentialsPath").Value;
            this.spreadsheetId = xDoc.Root.Element("SpreadsheetId").Value;
            this.dataSheetId = Convert.ToInt32(xDoc.Root.Element("DataSheetId").Value);
        }

        public async Task Write(BankDataSheet data)
        {
            SheetsService service = this.GetSheetsService();
            await this.ResetSorting(service.Spreadsheets);
            
            List<BankEntry> entries = await this.GetEntriesToAppend(service, data);

            if (entries.Any())
            {
                await this.AddEntries(service.Spreadsheets, entries);
            }

            int skippedCount = await this.VerifyAnyDataSkipped(service, data);
            if (skippedCount > 0)
            {
                Console.WriteLine($"{skippedCount} entries were missing. Verifying they were re-added correctly.");

                skippedCount = await this.VerifyAnyDataSkipped(service, data);
                if (skippedCount > 0)
                {
                    throw new InvalidOperationException($"Failed to re-add {skippedCount} entries.");                
                }
                else
                {
                    Console.WriteLine($"All entries re-added correctly.");
                }
            }

            await this.AddCategories(service.Spreadsheets, data.Categories);

        }
        
        private async Task ResetSorting(SpreadsheetsResource spreadsheets)
        {
            ValueRange response = await spreadsheets.Values.Get(this.spreadsheetId, this.headerRange).ExecuteAsync();
            this.LoadHeaderIndexes(response.Values.FirstOrDefault());
            
            List<Request> list = new List<Request>();

            ClearBasicFilterRequest clearFilter = new ClearBasicFilterRequest()
            {
                SheetId = this.dataSheetId
            };
            list.Add(new Request(){ ClearBasicFilter = clearFilter});

            
            SortRangeRequest sortRequest = new SortRangeRequest()
            {
                Range = new GridRange()
                {
                    SheetId    = this.dataSheetId,
                    StartColumnIndex = 0
                },
                SortSpecs = new List<SortSpec>()
                {
                    new SortSpec()
                    {
                        DimensionIndex = this.dateRow,
                        SortOrder = "DESCENDING"
                    }
                }
            };

            list.Add(new Request(){ SortRange = sortRequest});
            
            
            SetBasicFilterRequest setFilter = new SetBasicFilterRequest()
            {
                Filter = new BasicFilter()
                {
                    Range = new GridRange()
                    {
                        SheetId    = this.dataSheetId,
                        StartColumnIndex = 0
                    },
                }
            };
            list.Add(new Request(){ SetBasicFilter = setFilter});

            BatchUpdateSpreadsheetRequest updateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest()
            {
                Requests = list
            };

            BatchUpdateSpreadsheetResponse updateResponse = await spreadsheets.BatchUpdate(updateSpreadsheetRequest, this.spreadsheetId).ExecuteAsync();
        }

        private async Task<int> VerifyAnyDataSkipped(SheetsService sheetsService, BankDataSheet data)
        {
            ValueRange response = await sheetsService.Spreadsheets.Values.Get(this.spreadsheetId, this.readRange).ExecuteAsync();
            List<int> ids = response.Values
                .Skip(1)
                .Where(x=> !string.IsNullOrEmpty(x[0]?.ToString()))
                .Select(x=> Convert.ToInt32(x[0]))
                .ToList(); 
            
            List<int> duplicates = ids
                .GroupBy(i => i)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key).ToList();
            
            if(duplicates.Any())
            {
                Console.WriteLine($"Warning: found {duplicates.Count} duplicated IDs. {string.Join(", ", duplicates)}");

                IEnumerable<BankEntry> entries = data.Entries.Where(x => duplicates.Contains(x.BankEntryId));

                foreach (BankEntry bankEntry in entries)
                {
                    Console.WriteLine($"Duplicated entry: {bankEntry}");
                }
            }

            IEnumerable<BankEntry> entriesToVerify = data.Entries.Where(x => (DateTime.Today - x.Date).TotalDays < 30);

            List<BankEntry> missingEntries = new List<BankEntry>();
            foreach (BankEntry bankEntry in entriesToVerify)
            {
                if (ids.All(x => x != bankEntry.BankEntryId))
                {
                    missingEntries.Add(bankEntry);
                    Console.WriteLine($"Missing entry: {bankEntry}");
                }
            }

            await this.AddEntries(sheetsService.Spreadsheets, missingEntries);

            return missingEntries.Count;
        }

        private void LoadHeaderIndexes(IList<object> headerRow)
        {
            for (int i = 0; i < headerRow.Count; i++)
            {
                string text = headerRow[i]?.ToString()??"";
                switch (text)
                {
                    case "Entry ID":
                        this.entryIdRow = i;
                        break;
                    case "Account":
                        this.accountRow = i;
                        break;
                    case "Date":
                        this.dateRow = i;
                        break;
                    case "Currency":
                        this.currencyRow = i;
                        break;
                    case "Amount":
                        this.amountRow = i;
                        break;
                    case "Balance":
                        this.balanceRow = i;
                        break;
                    case "PaymentType":
                        this.paymentTypeRow = i;
                        break;
                    case "Recipient":
                        this.recipientRow = i;
                        break;
                    case "Payer":
                        this.payerRow = i;
                        break;
                    case "Note":
                        this.noteRow = i;
                        break;
                    case "Category":
                        this.categoryRow = i;
                        break;
                    case "Subcategory":
                        this.subcategoryRow = i;
                        break;
                    case "Tags":
                        this.tagsRow = i;
                        break;
                    case "Full Details":
                        this.fullDetailsRow = i;
                        break;
                }
            }

            if (this.allHeadersIndexes.Any(x=>x == null))
            {
                throw new InvalidOperationException("Error: incorrect header rows in Excel file.");
            }
            
        }
        
        private async Task<List<BankEntry>> GetEntriesToAppend(SheetsService sheetsService, BankDataSheet data)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =sheetsService.Spreadsheets.Values.Get(this.spreadsheetId, this.readRange);
            ValueRange response = await request.ExecuteAsync();
            IList<object> firstDataRow = response.Values.Skip(1).FirstOrDefault(x=>x[0] != null && !string.IsNullOrEmpty(x[0].ToString())); //skip header
            if (firstDataRow == null)
            {
                return data.Entries;
            }
            else
            {
                int latestId = Convert.ToInt32(firstDataRow[0]);
                DateTime latestDate = Convert.ToDateTime(firstDataRow[2]);
                List<int> entriesInTheSheet = new List<int>();
                List<IList<object>> dataList = response.Values.Skip(1).ToList();
                Console.WriteLine($"Total entries loaded: {dataList.Count}. Latest entry and date: {latestId} : {latestDate.Date}.");

                Console.WriteLine($"Looking for entries to be added to the spreadsheet...");
                foreach (IList<object> row in dataList)
                {
                    string? entryIdString = row[this.entryIdRow.Value]?.ToString();
                    if (!string.IsNullOrEmpty(entryIdString))
                    {
                        try
                        {
                            int entryId = Convert.ToInt32(entryIdString);
                            entriesInTheSheet.Add(entryId);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine($"Cannot convert: [{entryIdString}] to entry ID. {e}");
                            throw;
                        }
                    }
                }

                List<BankEntry> missingEntries = data.Entries.Where(newEntry =>
                    entriesInTheSheet.All(sheetEntry => sheetEntry != newEntry.BankEntryId)).ToList();
                foreach (BankEntry bankEntry in missingEntries)
                {
                    Console.WriteLine($"Entry to be added: {bankEntry}");
                }
                Console.WriteLine($"Total entries to be added: {missingEntries.Count}");

                
                return missingEntries;
            }
        }

        private async Task AddCategories(SpreadsheetsResource spreadsheets, List<Category> categories)
        {
            SpreadsheetsResource.ValuesResource.ClearRequest request = spreadsheets.Values.Clear(new ClearValuesRequest(), this.spreadsheetId, "Categories!A1:Z");
            await request.ExecuteAsync();

            
            List<IList<object>> values = new List<IList<object>>();
            
            foreach (Category category in categories)
            {
                List<object> row = new List<object>();
                row.Add(category.Name);
                foreach (Subcategory subcategory in category.Subcategories)
                {
                    row.Add(subcategory.Name);
                }
                values.Add(row);
            }

            ValueRange valueRange = new ValueRange()
            {
                MajorDimension = "ROWS",
                Values = values
            };
            
            SpreadsheetsResource.ValuesResource.UpdateRequest update = spreadsheets.Values.Update(valueRange, this.spreadsheetId, $"Categories!A1:Z");
            
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            UpdateValuesResponse res = await update.ExecuteAsync();
        }

        private async Task AddEntries(SpreadsheetsResource spreadsheets, List<BankEntry> entries)
        {
            await this.AddRows(entries, spreadsheets);

            
            List<IList<object>> values = new List<IList<object>>();
            foreach (BankEntry bankEntry in entries)
            {
                values.Add(this.ConvertToRow(bankEntry));
            }
            
            ValueRange valueRange = new ValueRange()
            {
                MajorDimension = "ROWS",
                Values = values
            };

            SpreadsheetsResource.ValuesResource.UpdateRequest update = spreadsheets.Values.Update(valueRange, this.spreadsheetId, $"Data!A2:N{entries.Count+1}");
            
            update.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            UpdateValuesResponse res = await update.ExecuteAsync();
        }

        private async Task AddRows(List<BankEntry> entries, SpreadsheetsResource spreadsheets)
        {
            List<Request> list = new List<Request>();

            InsertDimensionRequest insertRow = new InsertDimensionRequest
            {
                Range = new DimensionRange()
                {
                    Dimension = "ROWS", 
                    StartIndex = 1,
                    EndIndex = entries.Count + 1,
                    SheetId = this.dataSheetId
                }
            };

            list.Add(new Request {InsertDimension = insertRow});
           
            BatchUpdateSpreadsheetRequest updateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest()
            {
                Requests = list
            };
            
            BatchUpdateSpreadsheetResponse response = await spreadsheets.BatchUpdate(updateSpreadsheetRequest, this.spreadsheetId).ExecuteAsync();
        }

        private IList<object> ConvertToRow(BankEntry entry)
        {
            List<object> obj = new List<object>();

            // ReSharper disable PossibleInvalidOperationException - these values were null checked when sheet was loaded 
            obj.Insert(this.entryIdRow.Value, entry.BankEntryId);
            obj.Insert(this.accountRow.Value, entry.Account);
             obj.Insert(this.dateRow.Value, entry.Date.Date.ToString("dd/MM/yyyy"));
             obj.Insert(this.currencyRow.Value, entry.Currency);
             obj.Insert(this.amountRow.Value, entry.Amount);
             obj.Insert(this.balanceRow.Value, entry.Balance);
             obj.Insert(this.paymentTypeRow.Value, entry.PaymentType);
             obj.Insert(this.recipientRow.Value,entry.Recipient);
             obj.Insert(this.payerRow.Value, entry.Payer);
             obj.Insert(this.noteRow.Value,entry.Note);
             obj.Insert(this.categoryRow.Value, entry.Category);
             obj.Insert(this.subcategoryRow.Value,entry.Subcategory);
             obj.Insert(this.tagsRow.Value,string.Join(";",entry.Tags));
             obj.Insert(this.fullDetailsRow.Value, entry.FullDetails);
             // ReSharper restore PossibleInvalidOperationException
                 
             return obj;
        }

        private SheetsService GetSheetsService()
        {
            UserCredential credential;

            using (FileStream stream =
                new FileStream(this.credentialsPath, FileMode.Open,
                    FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Sheets API service.
            SheetsService service = new SheetsService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
            return service;
        }

    }

    
}