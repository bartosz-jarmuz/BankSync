using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Enrichers.Allegro.Model;
using Newtonsoft.Json;

namespace BankSync.Exporters.Allegro
{
    public class AllegroDataDownloader
    {
        public AllegroDataDownloader()
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            this.client = new HttpClient(handler);
        }

        private readonly HttpClient client;
        public async Task<AllegroData> GetData(string path)
        {
            await Task.Delay(0);
            //to be implemented

            var fileContent = File.ReadAllText(path);

            try
            {

                return JsonConvert.DeserializeObject<AllegroData>(fileContent);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}
