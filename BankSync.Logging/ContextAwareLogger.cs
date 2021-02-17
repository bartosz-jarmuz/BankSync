using System;

namespace BankSync.Logging
{
    public class ContextAwareLogger : IBankSyncLogger
    {
        private readonly IBankSyncLogger[] loggers;

        public ContextAwareLogger(params IBankSyncLogger[] loggers)
        {
            this.loggers = loggers;
        }
        
        private string Timestamp => $"{DateTime.Now:dd-MM-yyyy HH:mm:ss.fff}";
        
        public void Debug(string message)
        {
            string prefixed = this.Timestamp + " - " + message;

            foreach (IBankSyncLogger logger in this.loggers)
            {
                logger.Debug(prefixed);
            }
        }

        public void Info(string message)
        {
            string prefixed = this.Timestamp + " - " + message;

            foreach (IBankSyncLogger logger in this.loggers)
            {
                logger.Info(prefixed);
            }
        }

        public void Warning(string message)
        {
            string prefixed = this.Timestamp + " - " + message;

            foreach (IBankSyncLogger logger in this.loggers)
            {
                logger.Warning(prefixed);
            }
        }

        public void Error(string message, Exception ex)
        {
            string prefixed = this.Timestamp + " - " + message;

            foreach (IBankSyncLogger logger in this.loggers)
            {
                logger.Error(prefixed, ex);
            }
        }

        public void LogProgress(string progress)
        {
            foreach (IBankSyncLogger logger in this.loggers)
            {
                logger.LogProgress(progress);
            }
        }

        public void EndLogProgress(string endProgressMessage)
        {
            foreach (IBankSyncLogger logger in this.loggers)
            {
                logger.EndLogProgress(endProgressMessage);
            }
        }

        public void StartLogProgress(string startProgressMessage)
        {
            string prefixed = this.Timestamp + " - " + startProgressMessage;

            foreach (IBankSyncLogger logger in this.loggers)
            {
                logger.StartLogProgress(prefixed);
            }
        }
    }
}