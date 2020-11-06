using System;
using System.Security;

namespace BankSync.Model
{
    public class BankCredentials
    {
        public string Id { get; set; }

        public SecureString Password { get; set; }
    }
}
