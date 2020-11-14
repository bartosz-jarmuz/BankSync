using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Exporters.Ipko.DTO;
using Newtonsoft.Json;

namespace BankSync.Exporters.Ipko
{
    public partial class IpkoDataDownloader
    {
       

        class AccountOperations
        {
            private readonly HttpClient client;
            private string sessionId;
            private readonly Sequence sequence;

            public AccountOperations(HttpClient client, string sessionId, Sequence sequence)
            {
                this.client = client;
                this.sessionId = sessionId;
                this.sequence = sequence;
            }


            public async Task<XDocument> GetAccountData(string account, DateTime startDate, DateTime endDate)
            {
                
                string accountTicket = await this.GetAccountOperationsDownloadTicket(account, startDate, endDate);
                XDocument document = await this.GetDocument(accountTicket);
                return document;
            }

            private async Task<XDocument> GetDocument(string ticket)
            {
                using (HttpRequestMessage requestMessage =
                    new HttpRequestMessage(HttpMethod.Post, $"https://www.ipko.pl/secure/ikd3/print/{ticket}"))
                {
                    requestMessage.Content = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("ticket_id",ticket),
                    new KeyValuePair<string, string>("ticketId",ticket),
                    new KeyValuePair<string, string>("ias_sid", this.sessionId),
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

            private async Task<string> GetAccountOperationsDownloadTicket(string account, DateTime startDate,
                DateTime endDate)
            {

                GetAccountCompletedOperationsRequest exportRequest = new GetAccountCompletedOperationsRequest(this.sessionId, account, startDate, endDate, this.sequence);
                using (HttpRequestMessage requestMessage =
                    new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/secure/ikd3/api/accounts/operations/completed/download"))
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(exportRequest));
                    requestMessage.Headers.Add("x-session-id", this.sessionId);
                    requestMessage.Headers.Add("x-ias-ias_sid", this.sessionId);
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

        }
    }



}
