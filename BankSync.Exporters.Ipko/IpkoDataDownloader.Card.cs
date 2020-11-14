using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BankSync.Exporters.Ipko.DTO;
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

            public CardOperations(HttpClient client, string sessionId, Sequence sequence)
            {
                this.client = client;
                this.sessionId = sessionId;
                this.sequence = sequence;

            }

            public async Task<XDocument> GetCardData(string cardNumber, DateTime startDate, DateTime endDate)
            {
                GetCardsInitResponse.Paycard paycard = await this.GetCard(cardNumber);
                await this.ExtendCardOperationsDaterange(paycard.id, startDate, endDate);
                string downloadTicket = await this.GetDownloadTicket(paycard.id, startDate, endDate);
                XDocument document = await this.GetDocument(downloadTicket);
                return document;
            }

            /// <summary>
            /// This call is needed, because without it, the download ticket does not respect the start and end dates 
            /// </summary>
            /// <param name="cardId"></param>
            /// <param name="startDate"></param>
            /// <param name="endDate"></param>
            /// <returns></returns>
            private async Task ExtendCardOperationsDaterange(string cardId, DateTime startDate, DateTime endDate)
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

            private async Task<string> GetDownloadTicket(string cardId, DateTime startDate, DateTime endDate)
            {

                GetCardOperationsDownloadTicketRequest exportDownloadTicketRequest = new GetCardOperationsDownloadTicketRequest(this.sessionId, cardId, startDate, endDate, this.sequence);
                using (HttpRequestMessage requestMessage =
                    new HttpRequestMessage(HttpMethod.Post, "https://www.ipko.pl/secure/ikd3/api/paycards/credit/completed/download"))
                {
                    var json = JsonConvert.SerializeObject(exportDownloadTicketRequest);
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
                    GetDownloadTicketResponse response = (GetDownloadTicketResponse)JsonConvert.DeserializeObject(stringified, typeof(GetDownloadTicketResponse));
                    return response.response.ticket_id;
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

            private async Task<GetCardsInitResponse.Paycard> GetCard(string cardNumber)
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

                    
                    foreach (GetCardsInitResponse.Paycard paycard in response.response.paycard_list)
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
