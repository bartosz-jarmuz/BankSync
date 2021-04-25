using System;
using System.IO;

namespace BankSync.Logging
{
    public class SimpleFileLogger : IBankSyncLogger
    {
        private readonly string filePath;

        public SimpleFileLogger(string filePath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            this.filePath = filePath;
        }

        public void Debug(string message)
        {
            File.AppendAllLines(this.filePath, new []{"DEBUG - " + message});
        }

        public void Info(string message)
        {
            File.AppendAllLines(this.filePath, new[] { "INFO - " + message });
        }

        public void Warning(string message)
        {
            File.AppendAllLines(this.filePath, new[] { "WARNING - " + message });


        }

        public void Error(string message, Exception ex)
        {
            File.AppendAllLines(this.filePath, new[] { "ERROR - " + message });

        }

        public void LogProgress(string progress)
        {
        }

        public void EndLogProgress(string startProgressMessage)
        {
            File.AppendAllLines(this.filePath, new[] { "PROGRESS - " + startProgressMessage });

        }

        public void StartLogProgress(string startProgressMessage)
        {
            File.AppendAllLines(this.filePath, new[] { "PROGRESS - " + startProgressMessage });

        }
    }
}