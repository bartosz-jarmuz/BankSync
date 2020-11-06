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
using BankSync.Model;
using BankSync.Utilities;
using Newtonsoft.Json;

namespace BankSyncRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var downloader = new IpkoDataDownloader(GetCredentials());
            var ipkoData = await downloader.GetData(GetStoredValue("AccountNumber").ToInsecureString(), new DateTime(2020,10,01), new DateTime(2020, 10, 31));
            Console.WriteLine("Data downloaded");

        }


        private static BankCredentials GetCredentials()
        {
            var login = GetStoredValue("Login");
            var password = GetStoredValue("Password");

            return new BankCredentials()
            {
                Id = login.ToInsecureString(),
                Password = password
            };
        }

        private static SecureString GetStoredValue(string key)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var setting = config.AppSettings.Settings[key]?.Value;
            if (setting == null)
            {
                Console.Write($"Provide {key} (will be stored encrypted).");
                setting = Console.ReadLine();
                var secure = setting.ToSecureString();
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
