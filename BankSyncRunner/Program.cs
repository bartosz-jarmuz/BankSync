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
using BankSync.Logging;
using BankSync.Model;
using BankSync.Writers.Excel;
using BankSync.Writers.GoogleSheets;
using BankSync.Writers.Json;

namespace BankSyncRunner
{
    class Program
    {
        private static IDataMapper mapper =
            new ConfigurableDataMapper(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Mappings.xml"));

        static DataEnricherExecutor enricher = new DataEnricherExecutor();
        static FileInfo servicesConfigFile = new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Accounts.xml");

        static FileInfo googleWriterConfigFile =
            new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Google\GoogleWriterSettings.xml");

        private static DateTime startTime = DateTime.Today.AddMonths(-12);
        private static DateTime endTime = DateTime.Today;
        private static IBankSyncLogger logger = new ContextAwareLogger(new ConsoleLogger());

        static async Task Main(string[] args)
        {
            try
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
                        logger.Error($"Error while processing service: {configService.Name}", ex);
                    }
                }

                BankDataSheet bankDataSheet = BankDataSheet.Consolidate(datasets);

                logger.Info("Data downloaded");

                enricher.LoadEnrichers(config, logger);
                await enricher.EnrichData(bankDataSheet, startTime, endTime);
                logger.Info("Data enriched");

                DataAnalyzersExecutor analyzersExecutor = new DataAnalyzersExecutor(logger);
                analyzersExecutor.AnalyzeData(bankDataSheet);
                logger.Info("Data categorized");

                await Write(bankDataSheet);

                logger.Info("All done!");
            }
            catch (Exception ex)
            {
                logger.Error("Unexpected error!",ex);
            }
            Console.ReadKey();

        }

        private static async Task ProcessServices(ServiceConfig configServiceConfig, List<BankDataSheet> datasets)
        {
            if (string.Equals(configServiceConfig.Name, "IPKO", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ServiceUser configServiceUser in configServiceConfig.Users)
                {
                    IBankDataExporter downloader = new IpkoDataDownloader(configServiceUser, mapper, logger);
                    BankDataSheet dataset = await downloader.GetData(startTime, endTime);
                    datasets.Add(dataset);
                    logger.Debug($"IPKO - Loaded total of {dataset.Entries.Count} entries for {configServiceUser.UserName}");
                }
            }

            if (string.Equals(configServiceConfig.Name, "Citibank", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ServiceUser configServiceUser in configServiceConfig.Users)
                {
                    IBankDataExporter downloader = new CitibankDataDownloader(configServiceUser, mapper);
                    BankDataSheet dataset = await downloader.GetData(startTime, endTime);
                    datasets.Add(dataset);
                    logger.Debug(
                        $"Citibank - Loaded total of {dataset.Entries.Count} entries  for {configServiceUser.UserName}");
                }
            }
        }


        private static async Task Write(BankDataSheet ipkoData)
        {
            List<IBankDataWriter> writers = new List<IBankDataWriter>();
            string path = GetOutputPath();
            writers.Add(new ExcelBankDataWriter(path + ".xlsx"));
            writers.Add(new JsonBankDataWriter(path + ".json"));
            writers.Add(new GoogleSheetsBankDataWriter(googleWriterConfigFile, logger));
            foreach (IBankDataWriter writer in writers)
            {
                try
                {
                    await writer.Write(ipkoData);
                    logger.Info($"Data written with with {writer.GetType().Name}");
                }
                catch (Exception ex)
                {
                    logger.Error($"Error while writing with {writer.GetType().Name}.", ex);
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