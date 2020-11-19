using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Config;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Exporters.Ipko.DTO;
using BankSync.Exporters.Ipko.Mappers;
using BankSync.Model;
using BankSync.Utilities;
using Newtonsoft.Json;
using static BankSync.Config.BankSyncConfig;

namespace BankSync.Exporters.Ipko
{
    public partial class IpkoDataDownloader : IBankDataExporter
    {
        public IpkoDataDownloader(ServiceUser serviceUserConfig, IDataMapper mapper)
        {
            this.credentials = serviceUserConfig.Credentials;
            this.serviceUserConfig = serviceUserConfig;
            this.xmlTransformer = new IpkoXmlDataTransformer(mapper);
            this.sequence = new Sequence();
            this.oldDataManager = new OldDataManager(serviceUserConfig, this.xmlTransformer, mapper);
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };
            this.client = new HttpClient(handler);
        }

        private readonly Credentials credentials;
        private readonly ServiceUser serviceUserConfig;
        private readonly IpkoXmlDataTransformer xmlTransformer;
        private readonly HttpClient client;
        private string sessionId;
        private readonly Sequence sequence;
        private readonly OldDataManager oldDataManager;


        public async Task<BankDataSheet> GetData(DateTime startTime, DateTime endTime)
        {
            var datasets = new List<BankDataSheet>();
            var oldData = this.oldDataManager.GetOldData();
            datasets.Add(oldData);
            foreach (Account account in this.serviceUserConfig.Accounts)
            {
                datasets.Add(await this.GetAccountData(account.Number, startTime, endTime));
            }

            foreach (Card card in this.serviceUserConfig.Cards)
            {
                datasets.Add(await this.GetCardData(card.Number, startTime, endTime));
            }

            return BankDataSheet.Consolidate(datasets);
        }

        private async Task<BankDataSheet> GetAccountData(string account, DateTime startDate, DateTime endDate)
        {
            this.sessionId = await this.LoginAndGetSessionId();

            AccountOperations accountOperations = new AccountOperations(this.client, this.sessionId, this.sequence);
            XDocument document = await accountOperations.GetAccountData(account, startDate, endDate);

            this.oldDataManager.StoreData(document, account, startDate, endDate);

            return this.xmlTransformer.TransformXml(document);
        }

      
        private async Task<BankDataSheet> GetCardData(string cardNumber, DateTime startDate, DateTime endDate)
        {
            this.sessionId = await this.LoginAndGetSessionId();
            var cardOperations = new CardOperations(this.client, this.sessionId, this.sequence);
            XDocument document = await cardOperations.GetCardData(cardNumber, startDate, endDate);
            
            this.oldDataManager.StoreData(document, cardNumber, startDate, endDate);

            return this.xmlTransformer.TransformXml(document);
        }

        private async Task<string> LoginAndGetSessionId()
        {
            if (!string.IsNullOrEmpty(this.sessionId))
            {
                return this.sessionId;
            }
            else
            {
                int _ = this.sequence.GetValue();
                HttpResponseMessage response1 = await this.client.PostAsJsonAsync("https://www.ipko.pl/ipko3/login",
                    new LoginStep1Request(this.credentials.Id));

                string stringified1 = await response1.Content.ReadAsStringAsync();

                LoginStep1Response step1Response =
                    (LoginStep1Response)JsonConvert.DeserializeObject(stringified1, typeof(LoginStep1Response));

                if (step1Response.state_id == "captcha")
                {
                    throw new InvalidOperationException(
                        "You need to go to browser, enter captcha and log in - then try again.");
                }

                LoginStep2Request step2Request = new LoginStep2Request(step1Response.flow_id, step1Response.token,
                    this.credentials.Password.ToInsecureString(), this.sequence);
                using (HttpRequestMessage requestMessage =
                    new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/ipko3/login"))
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(step2Request));
                    string h = response1.Headers
                        .First(x => x.Key.Equals("x-session-id", StringComparison.OrdinalIgnoreCase)).Value.First()
                        .ToString();
                    requestMessage.Headers.Add("x-session-id", h);
                    HttpResponseMessage response2 = await this.client.SendAsync(requestMessage);
                    string stringified2 = await response2.Content.ReadAsStringAsync();
                    LoginStep2Response step2Response =
                        (LoginStep2Response)JsonConvert.DeserializeObject(stringified2, typeof(LoginStep2Response));
                    if (!step2Response.finished )
                    {
                        throw new InvalidOperationException(
                            "You need to go to browser and log in - then try again.");
                    }
                    this.sessionId = response2.Headers
                        .First(x => x.Key.Equals("x-session-id", StringComparison.OrdinalIgnoreCase)).Value.First()
                        .ToString();
                    if (step2Response?.token == null)
                    {
                        throw new InvalidOperationException("Failed to log in");
                    }
                }

                return this.sessionId;
            }

        }



    }
}
