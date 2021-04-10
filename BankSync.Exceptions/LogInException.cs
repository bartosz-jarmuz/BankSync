using System;

namespace BankSync.Exceptions
{
    public class LogInException : Exception
    {
        public LogInException(Type serviceType, string message) : base(serviceType.Name + " - " + message)
        {
        }
    }
}
