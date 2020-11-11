using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BankSync.Enrichers.Allegro.Model;
using Newtonsoft.Json;

namespace BankSync.Enrichers.Allegro
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
