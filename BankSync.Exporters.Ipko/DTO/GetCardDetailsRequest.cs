// -----------------------------------------------------------------------
//  <copyright file="GetCardDetailsRequest.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

namespace BankSync.Exporters.Ipko.DTO
{
    public class GetCardDetailsRequest
    {
        public GetCardDetailsRequest(string sid, string objectId, Sequence sequence)
        {
            this.sid = sid;
            this.request = new Request(objectId);
            this.seq = sequence.GetValue();
        }

        public string _method { get; set; } = "GET";
        public string sid { get; set; }
        public int seq { get; set; } = 1;
        public string location { get; set; } = "";
        public Request request { get; set; }
        public class Request
        {
            public Request(string objectId)
            {
                this.object_id = objectId;
            }

            public string object_id { get; set; }
        }
    }
}