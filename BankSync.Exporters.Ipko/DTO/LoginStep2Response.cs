namespace BankSync.Exporters.Ipko.DTO
{

    public class LoginStep2Response
    {
        public string flow_id { get; set; }
        public int httpStatus { get; set; }
        public string state_id { get; set; }
        public Response response { get; set; }
        public string token { get; set; }
        public bool finished { get; set; }
        public class Response
        {
            public Data data { get; set; }
        }
        public class Data
        {
            public string login_type { get; set; }
        }
    }

    

   


}
