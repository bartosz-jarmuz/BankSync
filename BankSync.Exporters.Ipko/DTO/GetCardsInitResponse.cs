
// ReSharper disable InconsistentNaming

namespace BankSync.Exporters.Ipko.DTO
{

    public class GetCardsInitResponse
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
            public Paycard[] paycard_list { get; set; }
            public Paycard_Offers[] paycard_offers { get; set; }
            public object[] ghost_offers { get; set; }
            public Actions actions { get; set; }
            public string first_paycard_fee_info { get; set; }
            public bool remove_new_offer { get; set; }
            public Zxc_Offers zxc_offers { get; set; }
        }

        public class Actions
        {
            public bool can_order_debit_card { get; set; }
            public bool can_order_card { get; set; }
        }

        public class Zxc_Offers
        {
            public Place[] places { get; set; }
        }

        public class Place
        {
            public object[] items { get; set; }
            public string place_id { get; set; }
        }

        public class Paycard
        {
            public string style { get; set; }
            public bool active_company { get; set; }
            public string description { get; set; }
            public string holder { get; set; }
            public bool is_business { get; set; }
            public bool is_debit_card_for_user { get; set; }
            public string number { get; set; }
            public bool show_notifications { get; set; }
            public Available_Balance available_balance { get; set; }
            public string account_number { get; set; }
            public bool is_user_card { get; set; }
            public string type { get; set; }
            public string id { get; set; }
            public object is_multi_currency { get; set; }
        }

        public class Available_Balance
        {
            public string currency { get; set; }
            public float value { get; set; }
        }

        public class Paycard_Offers
        {
            public string text { get; set; }
            public string href { get; set; }
            public string id { get; set; }
        }
    }

 

}
