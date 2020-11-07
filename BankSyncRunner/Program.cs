using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Exporters.Ipko;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Exporters.Ipko.Mappers;
using BankSync.Model;
using BankSync.Utilities;
using BankSync.Writers.Csv;
using BankSync.Writers.Excel;
using Newtonsoft.Json;

namespace BankSyncRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IpkoDataTransformer transformer =
                new IpkoDataTransformer(new ConfigurableDataMapper(new FileInfo(@"C:\Users\bjarmuz\Documents\BankSync\Mappings.xml")));
            IpkoDataDownloader downloader = new IpkoDataDownloader(GetCredentials(), transformer);
            WalletDataSheet ipkoData = 
                await downloader.GetData(GetStoredValue("AccountNumber").ToInsecureString(), new DateTime(2020, 10, 01), new DateTime(2020, 10, 31));
            Console.WriteLine("Data downloaded");

            string outputPath = GetOutputPath();
            ExcelBankDataWriter writer = new ExcelBankDataWriter(outputPath);
            writer.Write(ipkoData);

            Console.WriteLine($"All written to: {outputPath}");


            Console.ReadKey();
        }

        static string GetOutputPath()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Output",
                DateTime.Now.ToString("yyyy-MM-dd HH_mm_ss") + ".xlsx");
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            return filePath;
        }

        private static BankCredentials GetCredentials()
        {
            SecureString login = GetStoredValue("Login");
            SecureString password = GetStoredValue("Password");

            return new BankCredentials()
            {
                Id = login.ToInsecureString(),
                Password = password
            };
        }

        private static SecureString GetStoredValue(string key)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string setting = config.AppSettings.Settings[key]?.Value;
            if (setting == null)
            {
                Console.Write($"Provide {key} (will be stored encrypted).");
                setting = Console.ReadLine();
                SecureString secure = setting.ToSecureString();
                config.AppSettings.Settings.Add(key, secure.EncryptString());
                config.Save(ConfigurationSaveMode.Modified);
                return secure;
            }
            else
            {
                return setting.DecryptString();
            }
        }

    }






}
