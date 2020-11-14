using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Exporters.Ipko.DTO;
using BankSync.Model;
using BankSync.Utilities;
using Newtonsoft.Json;

namespace BankSync.Exporters.Ipko
{
    public partial class IpkoDataDownloader
    {
        public IpkoDataDownloader(Credentials credentials, IpkoDataTransformer transformer)
        {
            this.credentials = credentials;
            this.transformer = transformer;
            this.cookies = new CookieContainer();
            this.sequence = new Sequence();
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = this.cookies
            };
            this.client = new HttpClient(handler);
        }

        private readonly Credentials credentials;
        private readonly IpkoDataTransformer transformer;
        private readonly HttpClient client;
        private string sessionId;
        private readonly CookieContainer cookies;
        private Sequence sequence;

        public async Task<WalletDataSheet> GetAccountData(string account, DateTime startDate, DateTime endDate)
        {
            this.sessionId = await this.LoginAndGetSessionId();

            AccountOperations accountOperations = new AccountOperations(this.client, this.sessionId, this.sequence);
            XDocument document = await accountOperations.GetAccountData(account, startDate, endDate);

            return this.transformer.Transform(document);
        }

        public async Task<WalletDataSheet> GetCardData(string cardNumber, DateTime startDate, DateTime endDate)
        {
            this.sessionId = await this.LoginAndGetSessionId();
            var cardOperations = new CardOperations(this.client, this.cookies, this.sessionId, this.sequence);
            XDocument document = await cardOperations.GetCardData(cardNumber, startDate, endDate);

            return this.transformer.Transform(document);
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

    public class Sequence
    {
        private int value;
        public int GetValue()
        {
            int toReturn = this.value;
            this.value++;
            return toReturn;
        }
    }
}
