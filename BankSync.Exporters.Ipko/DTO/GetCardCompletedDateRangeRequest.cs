﻿// -----------------------------------------------------------------------
//  <copyright file="GetCardDetailsRequest.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

using System;

namespace BankSync.Exporters.Ipko.DTO
{

    internal class GetCardCompletedDateRangeRequest
    {
        public GetCardCompletedDateRangeRequest(string sid, string cardId, DateTime startDate, DateTime endDate, Sequence sequence)
        {
            this.sid = sid;
            this.request = new Request(cardId, new Filter(startDate, endDate));
            this.seq = sequence.GetValue();
        }

        public string _method { get; set; } = "POST";
        public string sid { get; set; }
        public int seq { get; set; }
        public string location { get; set; } = "";
        public Request request { get; set; }
        public class Request
        {
            public Request(string objectId, Filter filter)
            {
                this.object_id = objectId;
                this.filter = filter;
            }

            public string object_id { get; set; }
            public Filter filter { get; set; }
        }

        public class Filter
        {
            public Filter(DateTime startDate, DateTime endDate)
            {
                this.date_from = startDate.ToString("yyyy-MM-dd");
                this.date_to = endDate.ToString("yyyy-MM-dd");
            }
            public string date_from { get; set; }
            public string date_to { get; set; }
            public string amount_greater { get; set; } = "";
            public string amount_smaller { get; set; } = "";
            public string operation_type { get; set; } = "";
        }
    }

   

}