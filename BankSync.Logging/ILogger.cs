using System;

namespace BankSync.Logging
{
    public interface IBankSyncLogger
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception ex);
    }
}