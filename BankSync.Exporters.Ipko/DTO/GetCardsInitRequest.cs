
// ReSharper disable InconsistentNaming

namespace BankSync.Exporters.Ipko.DTO
{

    internal class GetCardsInitRequest
    {

        public GetCardsInitRequest(Sequence sequence, string sessionId)
        {
            this.seq = sequence.GetValue();
            this.sid = sessionId;
        }

        public int seq { get; set; } 
        public string location { get; set; } = "";
        public string _method { get; set; } = "GET";
        public string sid { get; set; } 
        public Request request { get; set; } = new Request();
        public class Request
        {
            public Request()
            {
            }

            public string object_id { get; set; }
        }
    }
}
