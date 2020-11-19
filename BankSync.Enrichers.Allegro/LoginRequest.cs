// -----------------------------------------------------------------------
//  <copyright file="LoginRequest.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Enrichers.Allegro
{
    internal class LoginRequest
    {
        public LoginRequest(string username, string password)
        {
            this.username = username;
            this.password = password;
        }

        public string username { get; set; }
        public string password { get; set; }

        public string authorizationUri { get; set; } =
            "https://allegro.pl/auth/oauth/authorize?client_id=tb5SFf3cRxEyspDN&redirect_uri=https://allegro.pl/login/auth&response_type=code&state=3Bviuc";

        public string authenticationMethod { get; set; } = "CREDENTIALS";
    }
}