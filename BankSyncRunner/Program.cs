using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BankSyncRunner
{
    /// <summary>
    /// The program
    /// </summary>
    static class Program
    {
        static async Task Main()
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
            
            BankSyncConsoleRunner consoleRunner = new BankSyncConsoleRunner(workingFolderPath);

            await consoleRunner.Run();
        }

      
    }
}