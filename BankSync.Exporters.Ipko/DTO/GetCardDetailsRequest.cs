
// ReSharper disable InconsistentNaming

namespace BankSync.Exporters.Ipko.DTO
{

    public class GetCardDetailsRequest
    {
        public GetCardDetailsRequest(string sid)
        {
            this.sid = sid;
        }

        public string _method { get; set; }
        public string sid { get; set; }
        public int seq { get; set; } = 0;
        public string location { get; set; }
        public Request request { get; set; }
        public class Request
        {
        }
    }

    

}
