using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using BankSync.Config;
using BankSync.DataMapping;
using BankSync.Exceptions;
using BankSync.Exporters.Citibank;
using BankSync.Exporters.Ipko;
using BankSync.Logging;
using BankSync.Model;
using BankSync.Writers.Excel;
using BankSync.Writers.GoogleSheets;
using BankSync.Writers.Json;
using BankSyncRunner;
using Microsoft.Web.WebView2.Wpf;

namespace BankSync.Windows
{
    public class BankSyncWindowsRunner
    {
        public BankSyncWindowsRunner(string workingFolderPath, IBankSyncLogger windowsLogger, WebView2 browser)
        {
            this.logger = new ContextAwareLogger(windowsLogger, new SimpleFileLogger(Path.Combine(workingFolderPath, "Logs", $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}.log")));
            this.servicesConfigFile = new FileInfo(Path.Combine(workingFolderPath, @"Accounts.xml"));
            mapper =
                new ConfigurableDataMapper(new FileInfo(Path.Combine(workingFolderPath, @"Mappings.xml")));
            googleWriterConfigFile = new FileInfo(Path.Combine(workingFolderPath, @"Google\GoogleWriterSettings.xml"));
            analyzersExecutor = new DataAnalyzersExecutor(logger, new DirectoryInfo(workingFolderPath));
            enricher = new DataEnricherExecutor(logger, new WebBrowserAllegroDataDownloader(browser, this.logger));
            this.config = new BankSyncConfig(servicesConfigFile, GetInput);

        }

        private readonly DateTime startTime = DateTime.Today.AddMonths(-12);
        private readonly DateTime endTime = DateTime.Today;

        private readonly IDataMapper mapper;
        private readonly FileInfo servicesConfigFile;
        private readonly FileInfo googleWriterConfigFile;

        private readonly IBankSyncLogger logger;
        private readonly DataEnricherExecutor enricher;
        private readonly DataAnalyzersExecutor analyzersExecutor;
        private readonly BankSyncConfig config;

        public async Task<BankDataSheet> DownloadData()
        {
            try
            {
                List<BankDataSheet> datasets = new List<BankDataSheet>();
                logger.Info("SyncRunner started. Getting data from connected bank services...");
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

                logger.Info("Data downloaded.");
                return BankDataSheet.Consolidate(datasets);

            }
            catch (Exception ex)
            {
                logger.Error("Process failed!", ex);
                throw;
            }

        }

        public void EnrichData(BankDataSheet  bankDataSheet, Action allegroDownloadFinishedCallback)
        {
            try
            {
                logger.Info("Starting data enriching...");

                enricher.LoadEnrichers(config);
                enricher.EnrichData(bankDataSheet, startTime, endTime, enrichedData => Task.Run(()=>
                {
                    allegroDownloadFinishedCallback();
                    return this.AnalyzeAndSend(enrichedData);
                }));
            }
            catch (Exception ex)
            {
                logger.Error("Process failed!", ex);
                throw;
            }

        }

        public async Task AnalyzeAndSend(BankDataSheet bankDataSheet)
        {
            try
            {
                logger.Info("Data enriched. Proceeding to analysis and categorization...");
                analyzersExecutor.AnalyzeData(bankDataSheet);

                logger.Info("Data categorized. Proceeding to writing...");

                await Write(bankDataSheet);

                logger.Info("All done!");
            }
            catch (Exception ex)
            {
                logger.Error("Process failed!", ex);
            }

        }

        private async Task ProcessServices(ServiceConfig configServiceConfig, List<BankDataSheet> datasets)
        {
            if (string.Equals(configServiceConfig.Name, "IPKO", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ServiceUser configServiceUser in configServiceConfig.Users)
                {
                    try
                    {
                        IBankDataExporter downloader = new IpkoDataDownloader(configServiceUser, mapper, logger);
                        BankDataSheet dataset = await downloader.GetData(startTime, endTime);
                        datasets.Add(dataset);
                        logger.Debug(
                            $"IPKO - Loaded total of {dataset.Entries.Count} entries for {configServiceUser.UserName}");
                    }
                    catch (LogInException)
                    {
                        logger.Warning(
                            $"IPKO - {configServiceUser.UserName}. Could not log in.");
                    }
                    catch (Exception ex)
                    {
                        logger.Warning(
                            $"IPKO - Error while processing entries for {configServiceUser.UserName}. {ex}");
                    }
                }
            }

            if (string.Equals(configServiceConfig.Name, "Citibank", StringComparison.OrdinalIgnoreCase))
            {
                foreach (ServiceUser configServiceUser in configServiceConfig.Users)
                {
                    try
                    {
                        IBankDataExporter downloader = new CitibankDataDownloader(configServiceUser, mapper);
                        BankDataSheet dataset = await downloader.GetData(startTime, endTime);
                        datasets.Add(dataset);
                        logger.Debug(
                            $"Citibank - Loaded total of {dataset.Entries.Count} entries  for {configServiceUser.UserName}");
                    }
                    catch (LogInException)
                    {
                        logger.Warning(
                            $"Citibank - {configServiceUser.UserName}. Could not log in.");
                    }
                    catch (Exception ex)
                    {
                        logger.Warning(
                            $"Citibank - Error while processing entries for {configServiceUser.UserName}. {ex}");
                    }
                }
            }
        }


        private async Task Write(BankDataSheet ipkoData)
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
            Console.ForegroundColor = ConsoleColor.White;
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