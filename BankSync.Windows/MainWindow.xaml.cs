﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using BankSync.Model;
using BankSync.Utilities;
using BankSyncRunner;
using Microsoft.Extensions.Configuration;
using MessageBox = System.Windows.MessageBox;

namespace BankSync.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly TextBoxLogger logger;
        private Visibility webViewVisibility = Visibility.Collapsed;
        private DateTime startDate = DateTime.Now.AddMonths(-1).FirstDayOfMonth();

        public Visibility WebViewVisibility
        {
            get => webViewVisibility;
            set
            {
                webViewVisibility = value;
                this.OnPropertyChanged(nameof(WebViewVisibility));
            }
        }

        public DateTime StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                this.OnPropertyChanged(nameof(StartDate));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.logger = new TextBoxLogger(this.OutputBox);

        }

        private async void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            string workingFolderPath = config["WorkingFolderPath"];

            if (string.IsNullOrEmpty(workingFolderPath) || !Directory.Exists(workingFolderPath))
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                System.Windows.MessageBox.Show($"Failed to find working folder. App settings specify [{workingFolderPath}] as expected path.");
                return;
            }

            await this.Browser.EnsureCoreWebView2Async();

            logger.Info("Starting");
            BankSyncWindowsRunner runner = new BankSyncWindowsRunner(workingFolderPath, logger, this.Browser, this.StartDate);
            try
            {
               var data = await Task.Run(() => runner.DownloadData());
                MessageBoxResult getFreshAllegroData = MessageBox.Show("Get fresh data from Allegro?", "Confirmation needed", MessageBoxButton.YesNo);
                if (getFreshAllegroData == MessageBoxResult.Yes)
                {
                    this.WebViewVisibility = Visibility.Visible;
                }

                runner.EnrichData(data, getFreshAllegroData == MessageBoxResult.Yes, () => this.WebViewVisibility = Visibility.Collapsed);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BankSync runner error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}