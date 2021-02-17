using System;
using System.Runtime.CompilerServices;

namespace BankSync.Logging
{
    public class ConsoleLogger : IBankSyncLogger
    {
        public void LogProgress(string progress)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write(progress);
            Console.ForegroundColor = color;
        }

        public void EndLogProgress(string endProgressMessage)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(endProgressMessage);
            Console.ForegroundColor = color;
        }

        public void StartLogProgress(string startProgressMessage)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.Write(startProgressMessage);
            Console.ForegroundColor = color;
        }

        public void Debug(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }

        public void Info(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }

        public void Warning(string message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message);
            Console.ForegroundColor = color;
        }

        public void Error(string message, Exception ex)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message + " " + ex);
            Console.ForegroundColor = color;
        }
        
        
        
    }
    
}