using System;
// ReSharper disable InconsistentNaming

namespace BankSync.Exporters.Ipko.DTO
{

    public class GetAccountOperationsDownloadTicketRequest
    {
        public GetAccountOperationsDownloadTicketRequest(string sid, string account, DateTime startDate, DateTime endDate, Sequence sequence)
        {
            this.sid = sid;
            this.seq = sequence.GetValue();
            this.request = new Request(account, startDate, endDate);
        }

        public int version { get; set; } = 2;
        public int seq { get; set; }
        public string location { get; set; } = "";
        public Request request { get; set; }
        public string sid { get; set; }
        public string _method { get; set; } = "POST";


        public class Request
        {
            public Request(string account, DateTime startDate, DateTime endDate)
            {
                this.account = account;
                this.date_from = startDate.ToString("yyyy-MM-dd");
                this.date_to = endDate.ToString("yyyy-MM-dd");
            }

            public string date_to { get; set; } 
            public string date_from { get; set; } 
            public string operation_type { get; set; } = "ALL";
            public string amount_greater { get; set; } = "";
            public string amount_smaller { get; set; } = "";
            public string title { get; set; } = "";
            public string other_side_owner { get; set; } = "";
            public string other_side_account { get; set; } = "";
            public string account { get; set; } 
            public string format { get; set; } = "xml";
        }

    }

}
