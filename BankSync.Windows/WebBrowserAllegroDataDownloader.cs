using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BankSync.Config;
using BankSync.Enrichers.Allegro;
using BankSync.Enrichers.Allegro.Model;
using BankSync.Logging;
using HtmlAgilityPack;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;

//using Gecko;
//using Gecko.Events;

namespace BankSync.Windows
{
    public class WebBrowserAllegroDataDownloader : IAllegroDataDownloader
    {
        private readonly WebView2 browser;
        private readonly IBankSyncLogger logger;
        private Action<AllegroDataContainer> callback;
        private ServiceUser userConfig;
        private DateTime oldestEntry;
        private int currentOffset;
        private List<AllegroDataContainer> dataList ;
        private bool loadedAllData;
        private bool loggedInDuringThisRun;

        public WebBrowserAllegroDataDownloader(WebView2 browser, IBankSyncLogger logger)
        {
            this.browser = browser;
            this.logger = logger;
            this.browser.NavigationCompleted += Browser_NavigationCompleted;
        }

        public async Task GetData(ServiceUser config, DateTime oldestEntryToGet, Action<AllegroDataContainer> completionCallback)
        {
            this.dataList = new List<AllegroDataContainer>();
            this.loadedAllData = false;
            this.currentOffset = 0;
            this.userConfig = config;
            this.oldestEntry = oldestEntryToGet;
            this.callback = completionCallback;
            this.loggedInDuringThisRun = false;
            this.browser.CoreWebView2.Navigate("https://allegro.pl/logout.php");
            await Task.Delay(2000);
            this.browser.CoreWebView2.Navigate("https://allegro.pl/logowanie");
        }

        private async void Browser_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            var currentPage = await this.GetCurrentHtml();
            if (currentPage == null)
            {
                return;
            }
            if (loadedAllData)
            {
                return;
            }
            if (await this.IsLoggedIn(currentPage))
            {
                await this.LoadData();
            }
        }

        private async Task<bool> IsLoggedIn(string currentPage)
        {
            if (this.loggedInDuringThisRun && currentPage.Contains("header-username"))
            {
                return true;
            }

            if (this.browser.Source.ToString().Contains("/logowanie"))
            {
                await Task.Delay(1000);
                await SendInput("login", userConfig.Credentials.Id);
                this.loggedInDuringThisRun = true;
            }

            return false;
        }

        private async Task SendInput(string id, string value)
        {
            await this.browser.CoreWebView2.ExecuteScriptAsync($"document.getElementById('{id}').value = '{value}'");
        }

        private async Task LoadData()
        {
            if (this.browser.Source.ToString().Contains("moje-allegro/zakupy"))
            {
                var data = GetDataFromResponse(await this.GetCurrentHtml());
                dataList.Add(new AllegroDataContainer(data, userConfig.UserName));
                var oldestDateInCurrentBatch = AllegroDataContainer.GetOldestDate(data);
                if (oldestDateInCurrentBatch == AllegroDataContainer.GetOldestDate(data))
                {
                    this.loadedAllData = true;
                    AllegroDataContainer consolidated = AllegroDataContainer.Consolidate(this.dataList);
                    this.callback(consolidated);
                    return;
                }

                this.currentOffset += 25;
                this.browser.CoreWebView2.Navigate(GetOffsetedListUrl());
            }
            else
            {
                this.browser.CoreWebView2.Navigate(GetOffsetedListUrl());
            }
        }

        private string GetOffsetedListUrl()
        {
            return $"https://" +
                   $"allegro.pl/moje-allegro/zakupy/nowe-kupione?filter=all&limit=25&offset={this.currentOffset}&sort=orderdate&order=DESC&decorate=paymentInfo";
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

        

        private async Task<string> GetCurrentHtml()
        {
            var html = await this.browser.CoreWebView2.ExecuteScriptAsync("document.body.outerHTML");

            if (html == "null")
            {
                return null;
            }
            var htmldecoded = Regex.Unescape(html);
            return htmldecoded.Trim('"');
        }
   
    }
}
