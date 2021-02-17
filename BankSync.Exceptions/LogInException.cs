using System;

namespace BankSync.Exceptions
{
    public class LogInException : Exception
    {
        public LogInException(string message) : base(message)
        {
        }
    }
}
