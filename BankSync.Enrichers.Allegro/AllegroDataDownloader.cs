﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BankSync.Config;
using BankSync.Enrichers.Allegro.Model;
using BankSync.Utilities;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace BankSync.Enrichers.Allegro
{
    public class AllegroDataDownloader
    {
        private readonly ServiceUser userConfig;

        public AllegroDataDownloader(ServiceUser userConfig)
        {
            this.userConfig = userConfig;
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

        public async Task<AllegroData> GetData()
        {

            await this.LogIn();

            return await this.LoadData();
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
            }

        }

        private async Task<AllegroData> LoadData()
        {
            HttpResponseMessage response = await this.client.GetAsync("https://allegro.pl/moje-allegro/zakupy/kupione");
            string stringified = await response.Content.ReadAsStringAsync();
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(stringified);

            IEnumerable<HtmlNode> scripts = htmlDoc.DocumentNode.Descendants("script");
            HtmlNode myOrdersScript = scripts.Where(x => x.InnerHtml.Contains("myorders")).OrderByDescending(x=>x.InnerHtml.Length).FirstOrDefault();
            return JsonConvert.DeserializeObject<AllegroData>(myOrdersScript.InnerText);
        }


    }
}
