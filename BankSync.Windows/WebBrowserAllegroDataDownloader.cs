using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BankSync.Config;
using BankSync.Enrichers.Allegro;
using BankSync.Enrichers.Allegro.Model;
//using Gecko;
//using Gecko.Events;

namespace BankSync.Windows
{
    public class WebBrowserAllegroDataDownloader : IAllegroDataDownloader
    {
        private readonly WebBrowser browser;
        private Action<AllegroDataContainer> callback;

        public WebBrowserAllegroDataDownloader(WebBrowser browser)
        {
            this.browser = browser;
            //browser.DocumentCompleted += BrowserOnDocumentCompleted;
        }

        //private void BrowserOnDocumentCompleted(object? sender, GeckoDocumentCompletedEventArgs e)
        //{
        //    if (false)
        //    {
        //        callback(new AllegroDataContainer(new AllegroData(), "Boo"));
        //    }
        //}

      

        public void GetData(ServiceUser userConfig, DateTime oldestEntry, Action<AllegroDataContainer> completionCallback)
        {
            this.callback = completionCallback;
             this.browser.Navigate("https://allegro.pl/logowanie");

        }

   
    }
}
