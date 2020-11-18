using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using BankSync.Analyzers.AI;
using BankSync.Config;
using BankSync.Exporters.Ipko;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Exporters.Ipko.Mappers;
using BankSync.Model;
using BankSync.Utilities;
using BankSync.Writers.Excel;
using BankSync.Writers.Json;

namespace BankSyncRunner
{
    class Program
    {
        static IpkoDataTransformer transformer = new IpkoDataTransformer(new ConfigurableDataMapper(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Mappings.xml")));
        static DataEnricherExecutor enricher = new DataEnricherExecutor();
        static IBankDataAnalyzer analyzer = new AllIfsAnalyzer(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Tags.xml"));
        static FileInfo configFile = new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Accounts.xml");
        private static DateTime startTime = new DateTime(2019, 12, 01);
        private static DateTime endTime = new DateTime(2020, 11, 14);

        static async Task Main(string[] args)
        {

            var config = new BankSyncConfig(configFile, GetInput);

            var datasets = new List<WalletDataSheet>();
            foreach (Service configService in config.Services)
            {
                await ProcessServices(configService, datasets);
            }

            WalletDataSheet ipkoData = WalletDataSheet.Consolidate(datasets);

            Console.WriteLine("Data downloaded");

            enricher.LoadEnrichers();
            await enricher.EnrichData(ipkoData);

            analyzer.AssignCategories(ipkoData);

            Write(ipkoData);
            Console.ReadKey();
        }

        private static async Task ProcessServices(Service configService, List<WalletDataSheet> datasets)
        {
            if (string.Equals(configService.Name, "IPKO", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ServiceUser configServiceUser in configService.Users)
                {
                    IBankDataExporter downloader = new IpkoDataDownloader(configServiceUser, transformer);

                    datasets.Add( await downloader.GetData(startTime, endTime));
                }
            }
            else
            {
                throw new InvalidOperationException($"Unsupported service type: [{configService?.Name}]");
            }

        }


        private static void Write(WalletDataSheet ipkoData)
        {
            var path = GetOutputPath();
            IBankDataWriter writer = new ExcelBankDataWriter(path + ".xlsx");
            writer.Write(ipkoData);
            writer = new JsonBankDataWriter(path + ".json");
            writer.Write(ipkoData);
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
