using System;

// ReSharper disable InconsistentNaming

namespace BankSync.Exporters.Ipko.DTO
{

    public class GetCardOperationsDownloadTicketRequest
    {
        public GetCardOperationsDownloadTicketRequest(string sid, string cardId, DateTime startDate, DateTime endDate,
            Sequence sequence)
        {
            this.sid = sid;
            this.request = new Request(cardId, startDate, endDate);
            this.seq = sequence.GetValue();
        }

        public string _method { get; set; } = "POST";
        public string sid { get; set; }
        public int seq { get; set; } 
        public string location { get; set; } = "";
        public Request request { get; set; }

        public class Request
        {
            public Request(string objectId, DateTime startDate, DateTime endDate)
            {
                this.object_id = objectId;
                this.date_from = startDate.ToString("yyyy-MM-dd");
                this.date_to = endDate.ToString("yyyy-MM-dd");
            }

            public string object_id { get; set; }
            public string date_from { get; set; }
            public string date_to { get; set; }
            public string format { get; set; } = "xml";
        }

    }


}
