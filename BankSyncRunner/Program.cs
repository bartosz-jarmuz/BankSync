using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using BankSync.Analyzers.AI;
using BankSync.Exporters.Ipko;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Exporters.Ipko.Mappers;
using BankSync.Model;
using BankSync.Utilities;
using BankSync.Writers.Excel;

namespace BankSyncRunner
{
    class Program
    {
        static IpkoDataTransformer transformer = new IpkoDataTransformer(new ConfigurableDataMapper(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Mappings.xml")));
        static DataEnricherExecutor enricher = new DataEnricherExecutor();
        static IBankDataAnalyzer analyzer = new AllIfsAnalyzer(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Tags.xml"));
        static FileInfo configFile = new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Accounts.xml");
        private static DateTime startTime = new DateTime(2020, 10, 01);
        private static DateTime endTime = new DateTime(2020, 10, 31);

        static async Task Main(string[] args)
        {

            var config = new Config(configFile, GetInput);

            var datasets = new List<WalletDataSheet>();
            foreach (Config.Service configService in config.Services)
            {
                foreach (Config.User configServiceUser in configService.Users)
                {
                    IpkoDataDownloader downloader = new IpkoDataDownloader(configServiceUser.Credentials, transformer);

                    foreach (Config.Account account in configServiceUser.Accounts)
                    {
                        datasets.Add(await downloader.GetData(account.Number, startTime  , endTime));
                    }
                    foreach (Config.Card card in configServiceUser.Cards)
                    {
                        datasets.Add(await downloader.GetData(card.Number, startTime ,endTime));
                    }
                }
            }

            WalletDataSheet ipkoData = WalletDataSheet.Consolidate(datasets);

            Console.WriteLine("Data downloaded");

            enricher.LoadEnrichers();
            await enricher.EnrichData(ipkoData);

            analyzer.AddTags(ipkoData);

            ExcelBankDataWriter writer = new ExcelBankDataWriter(GetOutputPath());
            writer.Write(ipkoData);
            Console.ReadKey();
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
                DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss") + ".xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            return filePath;
        }
    }

}
