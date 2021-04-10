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
using BankSync.Exceptions;
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
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3)
                }, (exception, timeSpan, retryCount, context) =>
                {
                    if (retryCount > 1)
                    {
                        this.logger.Warning($"Exception while downloading data from Allegro for {userConfig.UserName}. Will retry. Attempt {retryCount} of 3. {exception.Message}");
                    }
                    else
                    {
                        this.logger.Debug($"Exception while downloading data from Allegro for {userConfig.UserName}. Will retry. Attempt {retryCount} of 3. {exception.Message}");
                    }
                })
                .ExecuteAsync(() =>
                {
                    Task<AllegroDataContainer> returnValue = this.LoadData(oldestEntry);
                    return returnValue;
                });

            this.logger.Debug("Successfully downloaded data.");
            return data;
        }

        private async Task LogIn()
        {
            
            using (HttpRequestMessage requestMessage =
                new HttpRequestMessage(HttpMethod.Get, "https://allegro.pl/logowanie"))
            {
                requestMessage.Headers.Add("Accept", "application/json, text/plain, */*");
                requestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                requestMessage.Headers.Add("Accept-Language", "pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7,la;q=0.6");

                requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0");

                requestMessage.Headers.Add("csrf-token", this.csrfToken);
                requestMessage.Headers.Add("dpr", "1");
                requestMessage.Headers.Add("origin", "https://allegro.pl"); 
                requestMessage.Headers.Add("referrer", "https://allegro.pl/logowanie");
                
                requestMessage.Headers.Add("sec-ch-ua", "Google Chrome\";v=\"89\", \"Chromium\";v=\"89\", \";Not A Brand\";v=\"99");
                requestMessage.Headers.Add("sec-ch-ua-mobile", "?0");
                requestMessage.Headers.Add("Sec-Fetch-Dest", "empty");
                requestMessage.Headers.Add("Sec-Fetch-Mode", "cors");
                requestMessage.Headers.Add("Sec-Fetch-Site", "same-origin");
                
                requestMessage.Headers.Add("x-fp", "POST");

                HttpResponseMessage response = await this.client.SendAsync(requestMessage);
                string stringified = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (stringified.Contains("captcha", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new LogInException(this.GetType(),"Log in request unsuccessful. Captcha might be required.");
                    }
                    throw new LogInException(this.GetType(),"Log in request unsuccessful.");
                }
                else
                {
                    this.logger.Debug($"Logged in to Allegro for: {this.userConfig.Credentials.Id}");
                }

            }
            
            var loginPageResponse = await this.client.GetAsync("");
            var loginPageResponseStringified = await loginPageResponse.Content.ReadAsStringAsync();
            
            IEnumerable<Cookie> responseCookies = this.cookies.GetCookies(new Uri("https://allegro.pl/logowanie")).Cast<Cookie>();
            foreach (Cookie cookie in responseCookies)
            {
                if (cookie.Name == "CSRF-TOKEN")
                {
                    this.csrfToken = cookie.Value;
                }
            }

            await Task.Delay(15000);

            LoginRequest payload = new LoginRequest(this.userConfig.Credentials.Id, this.userConfig.Credentials.Password.ToInsecureString());

            using (HttpRequestMessage requestMessage =
                new HttpRequestMessage(HttpMethod.Post, "https://allegro.pl/authentication/credentials/web/verification"))
            {
                string json = JsonConvert.SerializeObject(payload);
                requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                requestMessage.Headers.Add("Accept", "application/json, text/plain, */*");
                requestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                requestMessage.Headers.Add("Accept-Language", "pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7,la;q=0.6");

                requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0");

                requestMessage.Headers.Add("csrf-token", this.csrfToken);
                requestMessage.Headers.Add("dpr", "1");
                requestMessage.Headers.Add("origin", "https://allegro.pl"); 
                requestMessage.Headers.Add("referrer", "https://allegro.pl/logowanie");
                
                requestMessage.Headers.Add("sec-ch-ua", "Google Chrome\";v=\"89\", \"Chromium\";v=\"89\", \";Not A Brand\";v=\"99");
                requestMessage.Headers.Add("sec-ch-ua-mobile", "?0");
                requestMessage.Headers.Add("Sec-Fetch-Dest", "empty");
                requestMessage.Headers.Add("Sec-Fetch-Mode", "cors");
                requestMessage.Headers.Add("Sec-Fetch-Site", "same-origin");
                
                requestMessage.Headers.Add("x-fp", "POST");

                HttpResponseMessage response = await this.client.SendAsync(requestMessage);
                string stringified = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (stringified.Contains("captcha", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new LogInException(this.GetType(),"Log in request unsuccessful. Captcha might be required.");
                    }
                    throw new LogInException(this.GetType(),"Log in request unsuccessful.");
                }
                else
                {
                    this.logger.Debug($"Logged in to Allegro for: {this.userConfig.Credentials.Id}");
                }

            }

        }

        private async Task<AllegroDataContainer> LoadData(DateTime oldestEntryToDownload)
        {
            List<AllegroDataContainer> dataList = new List<AllegroDataContainer>();
            AllegroData data = await GetFirstBatchOfDataSkippingAnyIntermediatePages();

            if (data?.myorders != null)
            {
                this.logger.StartLogProgress("Downloading Allegro Data: ");
                this.logger.LogProgress("|");
                dataList.Add(new AllegroDataContainer(data, this.userConfig.UserName));

                int offset = 0;
                var oldestDateInCurrentBatch = AllegroDataContainer.GetOldestDate(data);
                int getOlderDateAttemptsCount = 0;
                do
                {
                    offset += 25;
                    HttpResponseMessage response = await this.client.GetAsync(
                        $"https://allegro.pl/moje-allegro/zakupy/nowe-kupione?filter=all&limit=25&offset={offset}&sort=orderdate&order=DESC&decorate=paymentInfo");
                    string stringified = await response.Content.ReadAsStringAsync();

                    data = GetDataFromResponse(stringified);
                    dataList.Add(new AllegroDataContainer(data, this.userConfig.UserName));
                    this.logger.LogProgress("|");
                    if (oldestDateInCurrentBatch == AllegroDataContainer.GetOldestDate(data))
                    {
                        getOlderDateAttemptsCount++;
                        if (getOlderDateAttemptsCount > 2)
                        {
                            throw new InvalidOperationException(
                                "There was a problem with fetching older data from Allegro. There might be a problem with getting older data from Allegro.");
                        }
                    }
                } while (AllegroDataContainer.GetOldestDate(data) > oldestEntryToDownload);
                this.logger.EndLogProgress("_");
            }
            else
            {
                throw new InvalidDataException($"Failed to load myorders data in the response body.");
            }

            return AllegroDataContainer.Consolidate(dataList);
        }


        /// <summary>
        /// It can happen that even though we are logged in, the first page that we try to access will be redirected to something else.
        /// Therefore, try accessing the page again (once), if the data is not loaded.
        /// </summary>
        /// <returns></returns>
        private async Task<AllegroData> GetFirstBatchOfDataSkippingAnyIntermediatePages()
        {
            HttpResponseMessage response = await this.client.GetAsync("https://allegro.pl/moje-allegro/zakupy/nowe-kupione?filter=all&limit=25&offset=0&sort=orderdate&order=DESC&decorate=paymentInfo");
            string stringified = await response.Content.ReadAsStringAsync();

            AllegroData data = GetDataFromResponse(stringified);

            if (data?.myorders == null)
            {
                response = await this.client.GetAsync("https://allegro.pl/moje-allegro/zakupy/nowe-kupione?filter=all&limit=25&offset=0&sort=orderdate&order=DESC&decorate=paymentInfo");
                stringified = await response.Content.ReadAsStringAsync();
                data = GetDataFromResponse(stringified);
                if (data != null)
                {
                    this.logger.Debug("Loaded data on second attempt");
                }
            }
            return data;
        }


        private AllegroData GetDataFromResponse(string stringified)
        {
            try
            {

                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(stringified);

                IEnumerable<HtmlNode> scripts = htmlDoc.DocumentNode.Descendants("script");
                HtmlNode myOrdersScript = scripts.Where(x => x.InnerHtml.Contains("myorders"))
                    .OrderByDescending(x => x.InnerHtml.Length).FirstOrDefault();
                if (myOrdersScript == null)
                {
                    this.logger.Warning("Failed to find myorders script page in the response body.");   
                    return null;

                }
                return JsonConvert.DeserializeObject<AllegroData>(myOrdersScript.InnerText);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Failed to find proper data in the response body: {stringified}", ex);
            }
        }
    }
}
