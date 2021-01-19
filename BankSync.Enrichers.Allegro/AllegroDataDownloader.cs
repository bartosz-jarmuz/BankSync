using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BankSync.Config;
using BankSync.Enrichers.Allegro.Model;
using BankSync.Logging;
using BankSync.Utilities;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Polly;

namespace BankSync.Enrichers.Allegro
{
    internal class AllegroDataDownloader
    {
        private readonly ServiceUser userConfig;
        private readonly IBankSyncLogger logger;

        public AllegroDataDownloader(ServiceUser userConfig, IBankSyncLogger logger)
        {
            this.userConfig = userConfig;
            this.logger = logger;
            this.cookies = new CookieContainer();
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            handler.CookieContainer = this.cookies;
          

            this.client = new HttpClient(handler);
        }

        private readonly HttpClient client;
        private readonly CookieContainer cookies;
        private string csrfToken;

        public async Task<AllegroDataContainer> GetData(DateTime oldestEntry)
        {
            await this.LogIn();

            AllegroDataContainer data = await Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(new []
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                }, (exception, timeSpan, retryCount, context) => {
                    if (retryCount > 1)
                    {
                        this.logger.Warning($"Exception while downloading data from Allegro. Will retry. Attempt {retryCount} of 3");
                    }
                    else
                    {
                        this.logger.Debug($"Exception while downloading data from Allegro. Will retry. Attempt {retryCount} of 3");
                    }
                })
                .ExecuteAsync(()=>
                {
                    Task<AllegroDataContainer> returnValue = this.LoadData(oldestEntry);
                    this.logger.Debug("Successfully downloaded data.");
                    return returnValue;
                });
            
            return data;
        }

        private async Task LogIn()
        {
            await this.client.GetAsync("https://allegro.pl/login/form?authorization_uri=https%3A%2F%2Fallegro.pl%2Fauth%2Foauth%2Fauthorize%3Fclient_id%3Dtb5SFf3cRxEyspDN%26redirect_uri%3Dhttps%3A%2F%2Fallegro.pl%2Flogin%2Fauth%26response_type%3Dcode%26state%3DWwARxT&oauth=true");

            IEnumerable<Cookie> responseCookies = this.cookies.GetCookies(new Uri("https://allegro.pl/login/form")).Cast<Cookie>();
            foreach (Cookie cookie in responseCookies)
            {
                if (cookie.Name == "CSRF-TOKEN")
                {
                    this.csrfToken = cookie.Value;
                }
            }


            LoginRequest payload = new LoginRequest(this.userConfig.Credentials.Id, this.userConfig.Credentials.Password.ToInsecureString());

            using (HttpRequestMessage requestMessage =
                new HttpRequestMessage(HttpMethod.Post, "https://allegro.pl/login/authenticate"))
            {
                string json = JsonConvert.SerializeObject(payload);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Headers.Add("Accept", "application/json, text/plain, */*");
                requestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                requestMessage.Headers.Add("Accept-Language", "pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7,la;q=0.6");

                requestMessage.Headers.Add("Origin", "https://allegro.pl");

                requestMessage.Headers.Add("Sec-Fetch-Dest", "empty");
                requestMessage.Headers.Add("Sec-Fetch-Mode", "cors");
                requestMessage.Headers.Add("Sec-Fetch-Site", "same-origin");

                requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0");
                
                requestMessage.Headers.Add("csrf-token", this.csrfToken);
                requestMessage.Headers.Add("dpr", "1");
                requestMessage.Headers.Add("x-fp", "POST");

                HttpResponseMessage response = await this.client.SendAsync(requestMessage);
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException("Error while logging to Allegro");
                }
                else
                {
                    this.logger.Debug($"Logged in to Allegro for: {this.userConfig.Credentials.Id}");
                }
                
            }

        }

        private async Task<AllegroDataContainer> LoadData(DateTime oldestEntry)
        {
            List<AllegroDataContainer> dataList = new List<AllegroDataContainer>();
            HttpResponseMessage response = await this.client.GetAsync("https://allegro.pl/moje-allegro/zakupy/kupione");
            AllegroData data = await GetDataFromResponse(response);
            dataList.Add(new AllegroDataContainer(data, this.userConfig.UserName));

            int limit = 25;
            int offset = 25;
            do
            {
                response = await this.client.GetAsync(
                    $"https://allegro.pl/moje-allegro/zakupy/kupione?limit={limit}&offset={offset}");
                data = await GetDataFromResponse(response);
                dataList.Add(new AllegroDataContainer(data, this.userConfig.UserName));
                offset += 25;
            } while (AllegroDataContainer.GetOldestDate(data) > oldestEntry);
            

            
            return AllegroDataContainer.Consolidate(dataList);
        }

    



        private static async Task<AllegroData> GetDataFromResponse(HttpResponseMessage response)
        {
            string stringified = "Not loaded yet";
            try
            {

                stringified= await response.Content.ReadAsStringAsync();
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(stringified);

                IEnumerable<HtmlNode> scripts = htmlDoc.DocumentNode.Descendants("script");
                HtmlNode myOrdersScript = scripts.Where(x => x.InnerHtml.Contains("myorders"))
                    .OrderByDescending(x => x.InnerHtml.Length).FirstOrDefault();
                return JsonConvert.DeserializeObject<AllegroData>(myOrdersScript.InnerText);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Failed to find proper data in the response body: {stringified}", ex);
            }
        }
    }
}
