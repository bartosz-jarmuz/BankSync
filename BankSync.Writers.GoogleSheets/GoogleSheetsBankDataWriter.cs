using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Model;
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
        private readonly string readRange = "Data!A1:N200";
        private readonly string spreadsheetId;
        private readonly string credentialsPath;
        private readonly int sheetId;

        private static readonly string[] Scopes = {SheetsService.Scope.Spreadsheets};
        private static readonly string ApplicationName = "BankSync";

        public GoogleSheetsBankDataWriter(FileInfo googleWriterConfigFile)
        {
            this.googleWriterConfigFile = googleWriterConfigFile;
            var xDoc = XDocument.Load(this.googleWriterConfigFile.FullName);
            this.credentialsPath = xDoc.Root.Element("CredentialsPath").Value;
            this.spreadsheetId = xDoc.Root.Element("SpreadsheetId").Value;
            this.sheetId = Convert.ToInt32(xDoc.Root.Element("SheetId").Value);
        }

       

        public async Task Write(BankDataSheet data)
        {
            SheetsService service = this.GetSheetsService();
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

                var entries = data.Entries.Where(x => duplicates.Contains(x.BankEntryId));

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
                    Console.WriteLine($"Missing entry {bankEntry}");
                }
            }

            await this.AddEntries(sheetsService.Spreadsheets, missingEntries);

            return missingEntries.Count;
        }

        private async Task<List<BankEntry>> GetEntriesToAppend(SheetsService sheetsService, BankDataSheet data)
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =sheetsService.Spreadsheets.Values.Get(this.spreadsheetId, this.readRange);
            ValueRange response = await request.ExecuteAsync();
            IList<object> value = response.Values.Skip(1).FirstOrDefault(x=>x[0] != null && !string.IsNullOrEmpty(x[0].ToString())); //skip header
            if (value == null)
            {
                return data.Entries;
            }
            else
            {
                int latestId = Convert.ToInt32(value[0]);
                DateTime latestDate = Convert.ToDateTime(value[2]);
                //that's the topmost entries details - lets find the last entry in the data we have, and import all newer
                Console.WriteLine($"Latest entry and date: {latestId} : {latestDate.Date}.");

                for (int i = 0; i < data.Entries.Count; i++)
                {
                    BankEntry entry = data.Entries[i];
                    //we check both ID and date, in case we have been fiddling with the data manually 
                    if (entry.BankEntryId == latestId && entry.Date.Date == latestDate.Date)
                    {
                        List<BankEntry> entries = data.Entries.Take(i).ToList();
                        Console.WriteLine($"Appending {entries.Count} entries");
                        return entries;
                    }
                }
            }
            Console.WriteLine("No entries to append");
            return new List<BankEntry>();
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

        private async Task AddEntries(SpreadsheetsResource spreadsheets, List<BankEntry> entries)
        {
            await this.AddRows(entries, spreadsheets);

            
            List<IList<object>> values = new List<IList<object>>();
            foreach (BankEntry bankEntry in entries)
            {
                values.Add(this.GetData(bankEntry));
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
                    SheetId = this.sheetId
                }
            };

            list.Add(new Request {InsertDimension = insertRow});
           
            BatchUpdateSpreadsheetRequest updateSpreadsheetRequest = new BatchUpdateSpreadsheetRequest()
            {
                Requests = list
            };
            
            BatchUpdateSpreadsheetResponse response = await spreadsheets.BatchUpdate(updateSpreadsheetRequest, this.spreadsheetId).ExecuteAsync();
        }


      

        private IList<object> GetData(BankEntry entry)
        {
            List<object> obj = new List<object>();

            obj.Add(entry.BankEntryId);
             obj.Add(entry.Account);
             obj.Add(entry.Date.Date.ToString("dd/MM/yyyy"));
             obj.Add(entry.Currency);
             obj.Add(entry.Amount);
             obj.Add(entry.Balance);
             obj.Add(entry.PaymentType);
             obj.Add(entry.Recipient);
             obj.Add(entry.Payer);
             obj.Add(entry.Note);
             obj.Add(entry.Category);
             obj.Add(entry.Subcategory);
             obj.Add(string.Join(";",entry.Tags));
             obj.Add(entry.FullDetails);
                 
             return obj;
        }
    }
}