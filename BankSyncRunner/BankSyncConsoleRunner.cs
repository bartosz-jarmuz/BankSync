//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Threading.Tasks;
//using BankSync.Config;
//using BankSync.DataMapping;
//using BankSync.Enrichers.Allegro;
//using BankSync.Exceptions;
//using BankSync.Exporters.Citibank;
//using BankSync.Exporters.Ipko;
//using BankSync.Logging;
//using BankSync.Model;
//using BankSync.Writers.Excel;
//using BankSync.Writers.GoogleSheets;
//using BankSync.Writers.Json;

//namespace BankSyncRunner
//{
//    public class BankSyncConsoleRunner
//    {
//        public BankSyncConsoleRunner(string workingFolderPath)
//        {
//            this.servicesConfigFile = new FileInfo(Path.Combine(workingFolderPath, @"Accounts.xml"));
//            mapper =
//                new ConfigurableDataMapper(new FileInfo(Path.Combine(workingFolderPath, @"Mappings.xml")));
//            googleWriterConfigFile = new FileInfo(Path.Combine(workingFolderPath, @"Google\GoogleWriterSettings.xml"));
//            analyzersExecutor = new DataAnalyzersExecutor(logger, new DirectoryInfo(workingFolderPath));
//            //enricher = new DataEnricherExecutor(logger, new HttpClientAllegroDataDownloader(logger));
//            //this implementation is broken because of difficult Allegro anti-bot mechanisms
//        }
        
//        private readonly DateTime startTime = DateTime.Today.AddMonths(-12);
//        private readonly DateTime endTime = DateTime.Today;

//        private readonly IDataMapper mapper;
//        private readonly FileInfo servicesConfigFile;
//        private readonly FileInfo googleWriterConfigFile;

//        private readonly IBankSyncLogger logger = new ContextAwareLogger(new ConsoleLogger());
//        private readonly DataEnricherExecutor enricher;
//        private readonly DataAnalyzersExecutor analyzersExecutor;

//        public async Task Run()
//        {
//            try
//            {
//                BankSyncConfig config = new BankSyncConfig(servicesConfigFile, GetInput);

//                List<BankDataSheet> datasets = new List<BankDataSheet>();
//                logger.Info("SyncRunner started. Getting data from connected bank services...");
//                foreach (ServiceConfig configService in config.Services)
//                {
//                    try
//                    {
//                        await ProcessServices(configService, datasets);
//                    }
//                    catch (Exception ex)
//                    {
//                        logger.Error($"Error while processing service: {configService.Name}", ex);
//                    }
//                }

//                BankDataSheet bankDataSheet = BankDataSheet.Consolidate(datasets);

//                logger.Info("Data downloaded. Starting data enriching...");

//                enricher.LoadEnrichers(config);
//                await enricher.EnrichData(bankDataSheet, startTime, endTime);
                
//                logger.Info("Data enriched. Proceeding to analysis and categorization...");
//                analyzersExecutor.AnalyzeData(bankDataSheet);
                
//                logger.Info("Data categorized. Proceeding to writing...");

//                await Write(bankDataSheet);

//                logger.Info("All done!");
//            }
//            catch (Exception ex)
//            {
//                logger.Error("Process failed!",ex);
//            }
//            Console.WriteLine("Press any key to exit.");
//            Console.ReadKey();

//        }
        
//        private async Task ProcessServices(ServiceConfig configServiceConfig, List<BankDataSheet> datasets)
//        {
//            if (string.Equals(configServiceConfig.Name, "IPKO", StringComparison.OrdinalIgnoreCase))
//            {
//                foreach (ServiceUser configServiceUser in configServiceConfig.Users)
//                {
//                    try
//                    {
//                        IBankDataExporter downloader = new IpkoDataDownloader(configServiceUser, mapper, logger);
//                        BankDataSheet dataset = await downloader.GetData(startTime, endTime);
//                        datasets.Add(dataset);
//                        logger.Debug(
//                            $"IPKO - Loaded total of {dataset.Entries.Count} entries for {configServiceUser.UserName}");
//                    }
//                    catch (LogInException)
//                    {
//                        logger.Warning(
//                            $"IPKO - {configServiceUser.UserName}. Could not log in.");
//                    }
//                    catch (Exception ex)
//                    {
//                        logger.Warning(
//                            $"IPKO - Error while processing entries for {configServiceUser.UserName}. {ex}");
//                    }
//                }
//            }

//            if (string.Equals(configServiceConfig.Name, "Citibank", StringComparison.OrdinalIgnoreCase))
//            {
//                foreach (ServiceUser configServiceUser in configServiceConfig.Users)
//                {
//                    try
//                    {
//                        IBankDataExporter downloader = new CitibankDataDownloader(configServiceUser, mapper);
//                        BankDataSheet dataset = await downloader.GetData(startTime, endTime);
//                        datasets.Add(dataset);
//                        logger.Debug(
//                            $"Citibank - Loaded total of {dataset.Entries.Count} entries  for {configServiceUser.UserName}");
//                    }
//                    catch (LogInException)
//                    {
//                        logger.Warning(
//                            $"Citibank - {configServiceUser.UserName}. Could not log in.");
//                    }
//                    catch (Exception ex)
//                    {
//                        logger.Warning(
//                            $"Citibank - Error while processing entries for {configServiceUser.UserName}. {ex}");
//                    }
//                }
//            }
//        }


//        private async Task Write(BankDataSheet ipkoData)
//        {
//            List<IBankDataWriter> writers = new List<IBankDataWriter>();
//            string path = GetOutputPath();
//            writers.Add(new ExcelBankDataWriter(path + ".xlsx"));
//            writers.Add(new JsonBankDataWriter(path + ".json"));
//            writers.Add(new GoogleSheetsBankDataWriter(googleWriterConfigFile, logger));
//            foreach (IBankDataWriter writer in writers)
//            {
//                try
//                {
//                    await writer.Write(ipkoData);
//                    logger.Info($"Data written with with {writer.GetType().Name}");
//                }
//                catch (Exception ex)
//                {
//                    logger.Error($"Error while writing with {writer.GetType().Name}.", ex);
//                }
//            }
//        }

//        private static string GetInput(string question)
//        {
//            Console.BackgroundColor = ConsoleColor.DarkGreen;
//            Console.ForegroundColor = ConsoleColor.White;
//            Console.WriteLine(question);
//            Console.BackgroundColor = ConsoleColor.Black;
//            return Console.ReadLine();
//        }


//        static string GetOutputPath()
//        {
//            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Output",
//                DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss"));
//            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
//            return filePath;
//        }
//    }
//}