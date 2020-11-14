// -----------------------------------------------------------------------
//  <copyright file="GetCardDetailsRequest.cs" >
// 
//  </copyright>
// -----------------------------------------------------------------------

// ReSharper disable InconsistentNaming
namespace BankSync.Exporters.Ipko.DTO
{
#pragma warning disable S101 // Types should be named in PascalCase

    public class GetCardDetailsResponse
    {
        public Session session { get; set; }
        public int httpStatus { get; set; }
        public Response response { get; set; }

        public class Response
        {
            public object is_multi_currency { get; set; }
            public string number { get; set; }
            public Actions actions { get; set; }
            public bool psd2 { get; set; }
            public string id { get; set; }
            public string style { get; set; }
            public string holder { get; set; }
            public Agreement_Data agreement_data { get; set; }
            public bool has_extended_details { get; set; }
            public string type { get; set; }
            public string description { get; set; }
            public bool nkk { get; set; }
            public Available_Balance1 available_balance { get; set; }
            public Last_Statement last_statement { get; set; }
            public bool is_user_card { get; set; }
            public bool is_debit_card_for_user { get; set; }
            public bool is_business { get; set; }
            public object[] places { get; set; }
            public Limits limits { get; set; }
            public bool show_notifications { get; set; }
            public string account_number { get; set; }
            public bool limit_net_kk { get; set; }
            public bool active_company { get; set; }
        }

        public class Actions
        {
            public bool can_limit_change { get; set; }
            public bool can_make_transfer { get; set; }
            public bool can_cancel { get; set; }
            public bool can_unblock_card { get; set; }
            public bool can_repay { get; set; }
            public bool can_credit_limit_change { get; set; }
            public bool can_set_pin { get; set; }
            public bool can_order_after_cancellation { get; set; }
            public bool can_limit_renew { get; set; }
            public bool can_apply_for_limit_change { get; set; }
            public bool can_activate { get; set; }
            public bool can_block_card { get; set; }
            public bool can_replacement_card { get; set; }
            public bool can_change_plastic { get; set; }
        }

        public class Agreement_Data
        {
            public object owner { get; set; }
            public Available_Balance available_balance { get; set; }
        }

        public class Available_Balance
        {
            public string currency { get; set; }
            public float value { get; set; }
        }

        public class Available_Balance1
        {
            public string currency { get; set; }
            public float value { get; set; }
        }

        public class Last_Statement
        {
            public string payment_due_day { get; set; }
            public Payment_Min_Amount payment_min_amount { get; set; }
        }

        public class Payment_Min_Amount
        {
            public string currency { get; set; }
            public float value { get; set; }
        }

        public class Limits
        {
            public Credit credit { get; set; }
        }

        public class Credit
        {
            public string currency { get; set; }
            public float value { get; set; }
        }

        public class Session
        {
            public string lifetime { get; set; }
            public string session_expiration_warn_popup_limit { get; set; }
            public string sid { get; set; }
        }
    }

   

  

#pragma warning restore S101 // Types should be named in PascalCase
}