using System.Security;

namespace BankSync.Model
{
    public class Credentials
    {
        public string Id { get; set; }

        public SecureString Password { get; set; }
    }
}
