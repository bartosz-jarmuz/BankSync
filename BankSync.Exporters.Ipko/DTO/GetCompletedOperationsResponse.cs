namespace BankSync.Exporters.Ipko.DTO
{

    public class GetCompletedOperationsResponse
    {
        public Session session { get; set; }
        public int httpStatus { get; set; }
        public Response response { get; set; }


        public class Session
        {
            public string lifetime { get; set; }
            public string session_expiration_warn_popup_limit { get; set; }
            public string sid { get; set; }
        }

        public class Response
        {
            public string ticket_id { get; set; }
        }
    }



}
