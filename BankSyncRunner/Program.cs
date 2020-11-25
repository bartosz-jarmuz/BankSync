using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using BankSync.Analyzers.AI;
using BankSync.Config;
using BankSync.DataMapping;
using BankSync.Exporters.Citibank;
using BankSync.Exporters.Ipko;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Model;
using BankSync.Utilities;
using BankSync.Writers.Excel;
using BankSync.Writers.GoogleSheets;
using BankSync.Writers.Json;

namespace BankSyncRunner
{
    class Program
    {
        private static IDataMapper mapper = new ConfigurableDataMapper(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Mappings.xml"));
        static DataEnricherExecutor enricher = new DataEnricherExecutor();
        static IBankDataAnalyzer analyzer = new AllIfsAnalyzer(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Tags.xml"));
        static FileInfo servicesConfigFile = new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Accounts.xml");
        static FileInfo googleWriterConfigFile = new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Google\GoogleWriterSettings.xml");
        private static DateTime startTime = DateTime.Today.AddMonths(-12);
        private static DateTime endTime = DateTime.Today;

        static async Task Main(string[] args)
        {

            BankSyncConfig config = new BankSyncConfig(servicesConfigFile, GetInput);

            List<BankDataSheet> datasets = new List<BankDataSheet>();
            foreach (ServiceConfig configService in config.Services)
            {
                try
                {
                    await ProcessServices(configService, datasets);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while processing service: {configService.Name}" + ex);
                }
            }

            BankDataSheet bankDataSheet = BankDataSheet.Consolidate(datasets);

            Console.WriteLine("Data downloaded");

            enricher.LoadEnrichers(config);
            await enricher.EnrichData(bankDataSheet, startTime, endTime);
            Console.WriteLine("Data enriched");

            analyzer.AssignCategories(bankDataSheet);
            Console.WriteLine("Data categorized");

            await Write(bankDataSheet);

            Console.WriteLine("All done!");
            #if !DEBUG
            Console.ReadKey();
#endif
        }

        private static async Task ProcessServices(ServiceConfig configServiceConfig, List<BankDataSheet> datasets)
        {
            if (string.Equals(configServiceConfig.Name, "IPKO", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ServiceUser configServiceUser in configServiceConfig.Users)
                {
                    IBankDataExporter downloader = new IpkoDataDownloader(configServiceUser, mapper);

                    datasets.Add( await downloader.GetData(startTime, endTime));
                }
            }
            if (string.Equals(configServiceConfig.Name, "Citibank", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ServiceUser configServiceUser in configServiceConfig.Users)
                {
                    IBankDataExporter downloader = new CitibankDataDownloader(configServiceUser, mapper);

                    datasets.Add( await downloader.GetData(startTime, endTime));
                }
            }
        }


        private static async Task Write(BankDataSheet ipkoData)
        {
            var writers = new List<IBankDataWriter>();
            string path = GetOutputPath();
            writers.Add(new ExcelBankDataWriter(path + ".xlsx"));
            writers.Add(new JsonBankDataWriter(path + ".json"));
            writers.Add(new GoogleSheetsBankDataWriter(googleWriterConfigFile));
            foreach (IBankDataWriter writer in writers)
            {
                try
                {
                    await writer.Write(ipkoData);
                    Console.WriteLine($"Data written with with {writer.GetType().Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error while writing with {writer.GetType().Name}: {ex}");
                }
            }
        }

        private static string GetInput(string question)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(question);
            Console.BackgroundColor = ConsoleColor.Black;
            return Console.ReadLine();
        }


        static string GetOutputPath()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Output",
                DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss"));
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            return filePath;
        }
    }

}
