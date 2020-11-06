namespace BankSync.Exporters.Ipko.DTO
{
    public class LoginStep1Request
    {
        public LoginStep1Request(string login)
        {
            this.data = new Data()
                { fingerprint = "44b387165fdc3a632d822519bf8a3995", login = login };
        }

        public int version { get; set; } = 3;
        public int seq { get; set; }
        public string location { get; set; } = "";
        public string state_id { get; set; } = "login";

        public Data data { get; set; } 

        public string action { get; set; } = "submit";
        public class Data
        {
            public string login { get; set; }
            public string fingerprint { get; set; }
        }
    }


}
