using System;

// ReSharper disable InconsistentNaming

namespace BankSync.Exporters.Ipko.DTO
{

    public class GetCardCompletedOperationsRequest
    {
        public GetCardCompletedOperationsRequest(string sid, string account, DateTime startDate, DateTime endDate)
        {
            this.sid = sid;
            this.request = new Request(account, startDate, endDate);
        }

        public string _method { get; set; } = "POST";
        public string sid { get; set; }
        public int seq { get; set; } = 4;
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
