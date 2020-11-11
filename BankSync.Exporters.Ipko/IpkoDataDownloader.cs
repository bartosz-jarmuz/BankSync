using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Exporters.Ipko.DTO;
using BankSync.Model;
using BankSync.Utilities;
using Newtonsoft.Json;

namespace BankSync.Exporters.Ipko
{
    public class IpkoDataDownloader
    {
        private readonly Credentials credentials;
        private readonly IpkoDataTransformer transformer;

        public IpkoDataDownloader(Credentials credentials, IpkoDataTransformer transformer)
        {
            this.credentials = credentials;
            this.transformer = transformer;
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            this.client = new HttpClient(handler);
        }

        private readonly HttpClient client;
        public async Task<WalletDataSheet> GetData(string account, DateTime startDate, DateTime endDate)
        {
            string sessionId = await this.LoginAndGetSessionId();
            string accountTicket = await this.GetAccountOperationsDownloadTicket(sessionId, account, startDate, endDate);
            //string cardTicket = await this.GetCardOperationsDownloadTicket(sessionId, "", startDate, endDate);
            XDocument document = await this.GetDocument(sessionId, accountTicket);
            //XDocument document2 = await this.GetDocument(sessionId, cardTicket);

            return this.transformer.Transform(document);
        }

        private async Task<XDocument> GetDocument(string sessionId, string ticket)
        {
            using (HttpRequestMessage requestMessage =
                new HttpRequestMessage(HttpMethod.Post, $"https://www.ipko.pl/secure/ikd3/print/{ticket}"))
            {
                requestMessage.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("ticket_id",ticket),
                    new KeyValuePair<string, string>("ticketId",ticket),
                    new KeyValuePair<string, string>("ias_sid",sessionId),
                });
                requestMessage.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                requestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                requestMessage.Headers.Add("Upgrade-Insecure-Requests", "1");
                requestMessage.Headers.Add("Host", "www.ipko.pl");
                requestMessage.Headers.Add("Origin", "https://www.ipko.pl");
                requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0");
                HttpResponseMessage httpResponseMessage = await this.client.SendAsync(requestMessage);
                string text = await httpResponseMessage.Content.ReadAsStringAsync();
                return XDocument.Parse(text);
            }
        }

        private async Task<string> GetAccountOperationsDownloadTicket(string sessionId, string account, DateTime startDate,
            DateTime endDate)
        {

            GetAccountCompletedOperationsRequest exportRequest = new GetAccountCompletedOperationsRequest(sessionId, account,startDate, endDate);
            using (HttpRequestMessage requestMessage =
                new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/secure/ikd3/api/accounts/operations/completed/download"))
            {
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(exportRequest));
                requestMessage.Headers.Add("x-session-id", sessionId);
                requestMessage.Headers.Add("x-ias-ias_sid", sessionId);
                requestMessage.Headers.Add("x-http-method", "POST");
                requestMessage.Headers.Add("x-http-method-override", "POST");
                requestMessage.Headers.Add("x-method-override", "POST");
                requestMessage.Headers.Add("x-requested-with", "XMLHttpRequest");
                HttpResponseMessage httpResponseMessage = await this.client.SendAsync(requestMessage);
                string stringified = await httpResponseMessage.Content.ReadAsStringAsync();
                GetCompletedOperationsResponse response = (GetCompletedOperationsResponse)JsonConvert.DeserializeObject(stringified, typeof(GetCompletedOperationsResponse));
                return response.response.ticket_id;
            }
        }

        private async Task<string> GetCardOperationsDownloadTicket(string sessionId, string card, DateTime startDate,
            DateTime endDate)
        {

            GetCardCompletedOperationsRequest exportRequest = new GetCardCompletedOperationsRequest(sessionId, card, startDate, endDate);
            using (HttpRequestMessage requestMessage =
                new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/secure/ikd3/api/paycards/credit/completed/download"))
            {
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(exportRequest));
                requestMessage.Headers.Add("x-session-id", sessionId);
                requestMessage.Headers.Add("x-ias-ias_sid", sessionId);
                requestMessage.Headers.Add("x-http-method", "POST");
                requestMessage.Headers.Add("x-http-method-override", "POST");
                requestMessage.Headers.Add("x-method-override", "POST");
                requestMessage.Headers.Add("x-requested-with", "XMLHttpRequest");
                HttpResponseMessage httpResponseMessage = await this.client.SendAsync(requestMessage);
                string stringified = await httpResponseMessage.Content.ReadAsStringAsync();
                GetCompletedOperationsResponse response = (GetCompletedOperationsResponse)JsonConvert.DeserializeObject(stringified, typeof(GetCompletedOperationsResponse));
                return response.response.ticket_id;
            }
        }


        private async Task<string> GetCardId(string sessionId, string card, DateTime startDate,
            DateTime endDate)
        {

            GetCardDetailsRequest request = new GetCardDetailsRequest(sessionId);
            using (HttpRequestMessage requestMessage =
                new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/secure/ikd3/api/paycards/init"))
            {
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request));
                requestMessage.Headers.Add("x-session-id", sessionId);
                requestMessage.Headers.Add("x-ias-ias_sid", sessionId);
                requestMessage.Headers.Add("x-http-method", "POST");
                requestMessage.Headers.Add("x-http-method-override", "POST");
                requestMessage.Headers.Add("x-method-override", "POST");
                requestMessage.Headers.Add("x-requested-with", "XMLHttpRequest");
                HttpResponseMessage httpResponseMessage = await this.client.SendAsync(requestMessage);
                string stringified = await httpResponseMessage.Content.ReadAsStringAsync();
                GetCardDetailsResponse response = (GetCardDetailsResponse)JsonConvert.DeserializeObject(stringified, typeof(GetCardDetailsResponse));
                return null;
            }
        }



        private async Task<string> LoginAndGetSessionId()
        {
            HttpResponseMessage response1 = await this.client.PostAsJsonAsync("https://www.ipko.pl/ipko3/login", new LoginStep1Request(this.credentials.Id));

            string stringified1 = await response1.Content.ReadAsStringAsync();

            LoginStep1Response step1Response = (LoginStep1Response)JsonConvert.DeserializeObject(stringified1, typeof(LoginStep1Response));

            LoginStep2Request step2Request = new LoginStep2Request(step1Response.flow_id, step1Response.token, this.credentials.Password.ToInsecureString());
            string sessionId;
            using (HttpRequestMessage requestMessage =
                new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/ipko3/login"))
            {
                requestMessage.Content = new StringContent(JsonConvert.SerializeObject(step2Request));
                string h = response1.Headers.First(x => x.Key.Equals("x-session-id", StringComparison.OrdinalIgnoreCase)).Value.First().ToString();
                requestMessage.Headers.Add("x-session-id", h);
                HttpResponseMessage response2 = await this.client.SendAsync(requestMessage);
                string stringified2 = await response2.Content.ReadAsStringAsync();
                LoginStep2Response step2Response = (LoginStep2Response)JsonConvert.DeserializeObject(stringified2, typeof(LoginStep2Response));
                sessionId = response2.Headers.First(x => x.Key.Equals("x-session-id", StringComparison.OrdinalIgnoreCase)).Value.First().ToString();
                if (step2Response?.token == null)
                {
                    throw new InvalidOperationException("Failed to log in");
                }
            }

            return sessionId;
        }

    }



}
