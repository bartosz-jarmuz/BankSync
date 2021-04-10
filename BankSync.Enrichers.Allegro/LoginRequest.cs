// -----------------------------------------------------------------------
//  <copyright file="LoginRequest.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Enrichers.Allegro
{
    internal class LoginRequest
    {
        public LoginRequest(string login, string password)
        {
            this.login = login;
            this.password = password;
        }

        public string login { get; set; }
        public string password { get; set; }

        public string originUrl { get; set; } = "/smart";

    //    public string authorizationUri { get; set; } =
         //   "https://allegro.pl/auth/oauth/authorize?client_id=tb5SFf3cRxEyspDN&redirect_uri=https://allegro.pl/login/auth&response_type=code&state=3Bviuc";

    //    public string authenticationMethod { get; set; } = "CREDENTIALS";
    }
}