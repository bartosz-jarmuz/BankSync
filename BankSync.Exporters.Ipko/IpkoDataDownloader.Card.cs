using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
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
      
        class CardOperations
        {
            private readonly HttpClient client;
            private string sessionId;
            private Sequence sequence;

            public CardOperations(HttpClient client, CookieContainer cookies, string sessionId, Sequence sequence)
            {
                this.client = client;
                this.sessionId = sessionId;
                this.sequence = sequence;

            }

            public async Task<XDocument> GetCardData(string cardNumber, DateTime startDate, DateTime endDate)
            {
                Paycard paycard = await this.GetCard(cardNumber);
                await this.GetCardOperationsDaterange(paycard.id, startDate, endDate);
                string downloadTicket = await this.GetCardOperationsDownloadTicket(paycard.id, startDate, endDate);
                XDocument document = await this.GetDocument(downloadTicket);
                return document;
            }


            private async Task GetCardOperationsDaterange(string cardId, DateTime startDate, DateTime endDate)
            {

                GetCardCompletedDateRangeRequest exportRequest = new GetCardCompletedDateRangeRequest(this.sessionId, cardId, startDate, endDate, this.sequence);
                using (HttpRequestMessage requestMessage =
                    new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/secure/ikd3/api/paycards/credit/completed"))
                {
                    string json = JsonConvert.SerializeObject(exportRequest);
                    requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    requestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    requestMessage.Headers.Add("Accept-Language", "pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7,la;q=0.6");
                    requestMessage.Headers.Add("Connection", "keep-alive");

                    requestMessage.Headers.Add("Host", "www.ipko.pl");
                    requestMessage.Headers.Add("Origin", "https://www.ipko.pl");
                    requestMessage.Headers.Add("Referer", "https://www.ipko.pl/");


                    requestMessage.Headers.Add("Sec-Fetch-Dest", "empty");
                    requestMessage.Headers.Add("Sec-Fetch-Mode", "cors");
                    requestMessage.Headers.Add("Sec-Fetch-Site", "same-origin");

                    requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0");


                    requestMessage.Headers.Add("x-ias-ias_sid", this.sessionId);
                    requestMessage.Headers.Add("X-HTTP-Method", "POST");
                    requestMessage.Headers.Add("X-HTTP-Method-Override", "POST");
                    requestMessage.Headers.Add("X-METHOD-OVERRIDE", "POST");
                    requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

                    HttpResponseMessage _ = await this.client.SendAsync(requestMessage);
                }
            }

            private async Task<string> GetCardOperationsDownloadTicket(string cardId, DateTime startDate, DateTime endDate)
            {

                GetCardCompletedOperationsRequest exportRequest = new GetCardCompletedOperationsRequest(this.sessionId, cardId, startDate, endDate, this.sequence);
                using (HttpRequestMessage requestMessage =
                    new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/secure/ikd3/api/paycards/credit/completed/download"))
                {
                    var json = JsonConvert.SerializeObject(exportRequest);
                    requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    requestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    requestMessage.Headers.Add("Accept-Language", "pl-PL,pl;q=0.9,en-US;q=0.8,en;q=0.7,la;q=0.6");
                    requestMessage.Headers.Add("Connection", "keep-alive");

                    requestMessage.Headers.Add("Host", "www.ipko.pl");
                    requestMessage.Headers.Add("Origin", "https://www.ipko.pl");
                    requestMessage.Headers.Add("Referer", "https://www.ipko.pl/");


                    requestMessage.Headers.Add("Sec-Fetch-Dest", "empty");
                    requestMessage.Headers.Add("Sec-Fetch-Mode", "cors");
                    requestMessage.Headers.Add("Sec-Fetch-Site", "same-origin");

                    requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0");


                    requestMessage.Headers.Add("x-ias-ias_sid", this.sessionId);
                    requestMessage.Headers.Add("X-HTTP-Method", "POST");
                    requestMessage.Headers.Add("X-HTTP-Method-Override", "POST");
                    requestMessage.Headers.Add("X-METHOD-OVERRIDE", "POST");
                    requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");

                    HttpResponseMessage httpResponseMessage = await this.client.SendAsync(requestMessage);
                    string stringified = await httpResponseMessage.Content.ReadAsStringAsync();
                    GetCompletedOperationsResponse response = (GetCompletedOperationsResponse)JsonConvert.DeserializeObject(stringified, typeof(GetCompletedOperationsResponse));
                    return response.response.ticket_id;
                }
            }


            private async Task GetCardDetails(Paycard card)
            {
                GetCardDetailsRequest request = new GetCardDetailsRequest(this.sessionId, card.id, this.sequence);

                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post,
                    "https://www.ipko.pl/secure/ikd3/api/paycards/credit/details"))
                {
                    var json = JsonConvert.SerializeObject(request);
                    requestMessage.Content = new StringContent(json, Encoding.UTF8, "application/json");

                    requestMessage.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                    requestMessage.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                    requestMessage.Headers.Add("Connection", "keep-alive");

                    requestMessage.Headers.Add("Host", "www.ipko.pl");
                    requestMessage.Headers.Add("Origin", "https://www.ipko.pl");
                    requestMessage.Headers.Add("Referer", "https://www.ipko.pl/");

                    requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:79.0) Gecko/20100101 Firefox/79.0");


                    requestMessage.Headers.Add("x-ias-ias_sid", this.sessionId);
                    requestMessage.Headers.Add("X-HTTP-Method", "GET");
                    requestMessage.Headers.Add("X-HTTP-Method-Override", "GET");
                    requestMessage.Headers.Add("X-METHOD-OVERRIDE", "GET");
                    requestMessage.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    HttpResponseMessage httpResponseMessage = await this.client.SendAsync(requestMessage);
                    string stringified = await httpResponseMessage.Content.ReadAsStringAsync();
                    GetCardDetailsResponse response = (GetCardDetailsResponse)JsonConvert.DeserializeObject(stringified, typeof(GetCardDetailsResponse));
                    
                    
                }
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



         


            private async Task<Paycard> GetCard(string cardNumber)
            {
                GetCardsInitRequest request = new GetCardsInitRequest(this.sequence, this.sessionId);
                using (HttpRequestMessage requestMessage =
                    new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/secure/ikd3/api/paycards/init"))
                {
                    requestMessage.Content = new StringContent(JsonConvert.SerializeObject(request));
                    requestMessage.Headers.Add("x-session-id", this.sessionId);
                    requestMessage.Headers.Add("x-ias-ias_sid", this.sessionId);
                    requestMessage.Headers.Add("x-http-method", "POST");
                    requestMessage.Headers.Add("x-http-method-override", "POST");
                    requestMessage.Headers.Add("x-method-override", "POST");
                    requestMessage.Headers.Add("x-requested-with", "XMLHttpRequest");
                    HttpResponseMessage httpResponseMessage = await this.client.SendAsync(requestMessage);
                    string stringified = await httpResponseMessage.Content.ReadAsStringAsync();
                    GetCardsInitResponse response = (GetCardsInitResponse)JsonConvert.DeserializeObject(stringified, typeof(GetCardsInitResponse));

                    
                    foreach (Paycard paycard in response.response.paycard_list)
                    {
                        if (MaskedInputRecognizer.IsMatch(cardNumber, paycard.number))
                        {
                            return paycard;
                        }
                    }

                    return null;
                }
            }
        }
    }



}
