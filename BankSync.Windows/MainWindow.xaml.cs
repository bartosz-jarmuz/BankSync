using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using BankSync.Model;
using BankSyncRunner;
using Microsoft.Extensions.Configuration;

namespace BankSync.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        

        private void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            string workingFolderPath = config["WorkingFolderPath"];

            if (string.IsNullOrEmpty(workingFolderPath) || !Directory.Exists(workingFolderPath))
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"Failed to find working folder. App settings specify [{workingFolderPath}] as expected path.");
                Console.ReadKey();
                return;
            }

            var logger = new TextBoxLogger(this.OutputBox);
            logger.Info("Starting");
            BankSyncWindowsRunner runner = new BankSyncWindowsRunner(workingFolderPath, logger, null);
            try
            {
               //var data = await Task.Run(() => runner.DownloadData());
               var data = new BankDataSheet();

                runner.EnrichData(data);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "BankSync runner error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}