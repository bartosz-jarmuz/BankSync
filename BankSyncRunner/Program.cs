﻿using System;
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
        private static IDataMapper mapper = new ConfigurableDataMapper(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Mappings.xml"));
        static DataEnricherExecutor enricher = new DataEnricherExecutor();
        static IBankDataAnalyzer analyzer = new AllIfsAnalyzer(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Tags.xml"));
        static FileInfo configFile = new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Accounts.xml");
        private static DateTime startTime = DateTime.Today.AddMonths(-1);
        private static DateTime endTime = DateTime.Today;

        static async Task Main(string[] args)
        {

            var config = new BankSyncConfig(configFile, GetInput);

            var datasets = new List<BankDataSheet>();
            foreach (ServiceConfig configService in config.Services)
            {
                await ProcessServices(configService, datasets);
            }

            BankDataSheet ipkoData = BankDataSheet.Consolidate(datasets);

            Console.WriteLine("Data downloaded");

            enricher.LoadEnrichers(config);
            await enricher.EnrichData(ipkoData, startTime, endTime);
            Console.WriteLine("Data enriched");

            analyzer.AssignCategories(ipkoData);
            Console.WriteLine("Data categorized");

            Write(ipkoData);
            Console.WriteLine("Data written");


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
        }


        private static void Write(BankDataSheet ipkoData)
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
