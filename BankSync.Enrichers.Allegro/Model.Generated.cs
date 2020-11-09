using System.Globalization;
using System;

namespace BankSync.Enrichers.Allegro.Model
{

    public class AllegroData
    {
        public Parameters parameters { get; set; }
        public Slots slots { get; set; }
        public Context context { get; set; }
    }

    public class Parameters
    {
        public string detailsUrl { get; set; }
        public Countrycode countryCode { get; set; }
        public int timeToShowCancelPaymentButton { get; set; }
        public bool forceFallbackAjaxUpdateForEmptyDS { get; set; }
        public Myorders myorders { get; set; }
    }

    public class Countrycode
    {
        public string code { get; set; }
    }

    public class Myorders
    {
        public Myorder[] myorders { get; set; }
        public int total { get; set; }
    }

    public class Myorder
    {
        public string id { get; set; }
        public string purchaseId { get; set; }
        public Seller seller { get; set; }
        public Offer[] offers { get; set; }
        public Delivery delivery { get; set; }
        public Totalcost totalCost { get; set; }
        public Payment payment { get; set; }
        public Surcharge surcharge { get; set; }
        public object coins { get; set; }
        public DateTime timestamp { get; set; }
        public DateTime createDate { get; set; }
        public DateTime orderDate { get; set; }
        public string invoiceAddressId { get; set; }
        public object missingDependencies { get; set; }
        public string orderVersion { get; set; }
        public string serviceCountry { get; set; }
        public bool hasRelatedOrders { get; set; }
        public bool markedAsPaid { get; set; }
        public bool hiddenInMyOrders { get; set; }
        public string boughtOnDevice { get; set; }
        public bool tooOldToPay { get; set; }
        public bool onlinePaymentAvailable { get; set; }
        public bool cancelled { get; set; }
        public bool announcement { get; set; }
        public bool readOnly { get; set; }
        public string messageToSeller { get; set; }
    }

    public class Seller
    {
        public string id { get; set; }
        public string login { get; set; }
    }

    public class Delivery
    {
        public string addressId { get; set; }
        public Cost cost { get; set; }
        public string name { get; set; }
        public string methodId { get; set; }
        public Paymenttype paymentType { get; set; }
        public Type type { get; set; }
        public string orderDeliveryId { get; set; }
        public Parcelstatusinfo parcelStatusInfo { get; set; }
        public bool selfCollect { get; set; }
        public Generaldelivery generalDelivery { get; set; }
        public Discount discount { get; set; }
    }

    public class Cost
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Paymenttype
    {
        public string name { get; set; }
        public string type { get; set; }
    }

    public class Type
    {
        public string name { get; set; }
        public string type { get; set; }
    }

    public class Parcelstatusinfo
    {
        public string status { get; set; }
        public string type { get; set; }
        public string label { get; set; }
        public string link { get; set; }
        public bool externalLink { get; set; }
    }

    public class Generaldelivery
    {
        public string name { get; set; }
        public string description { get; set; }
        public Address address { get; set; }
    }

    public class Address
    {
        public string street { get; set; }
        public string code { get; set; }
        public string city { get; set; }
    }

    public class Discount
    {
        public Originalcost originalCost { get; set; }
        public string type { get; set; }
        public string planType { get; set; }
    }

    public class Originalcost
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Totalcost
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Payment
    {
        public string id { get; set; }
        public string method { get; set; }
        public string methodId { get; set; }
        public string provider { get; set; }
        public string status { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public Amount amount { get; set; }
        public Buyerpaidamount buyerPaidAmount { get; set; }
        public bool installments { get; set; }
        public Availability availability { get; set; }
        public bool freeboxSubscriptionIncluded { get; set; }
        public bool multipleSellers { get; set; }
        public string mask { get; set; }
    }

    public class Amount
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Buyerpaidamount
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Availability
    {
        public bool available { get; set; }
        public string reason { get; set; }
    }

    public class Surcharge
    {
        public string status { get; set; }
    }

    public class Offer
    {
        public string id { get; set; }
        public string orderOfferId { get; set; }
        public Unitprice unitPrice { get; set; }
        public Originalprice originalPrice { get; set; }
        public int quantity { get; set; }
        public DateTime orderDate { get; set; }
        public string title { get; set; }
        public string friendlyUrl { get; set; }
        public string type { get; set; }
        public string imageUrl { get; set; }
        public string productId { get; set; }
        public bool gold { get; set; }
        public bool auction { get; set; }
        public bool pharmacy { get; set; }
        public Offerprice offerPrice { get; set; }
        public Rebate rebate { get; set; }
    }

    public class Unitprice
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Originalprice
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Offerprice
    {
        public string amount { get; set; }
        public string currency { get; set; }
    }

    public class Rebate
    {
        public string id { get; set; }
        public Label label { get; set; }
    }

    public class Label
    {
        public string badge { get; set; }
        public string message { get; set; }
    }

    public class Slots
    {
        public object additionalBanner { get; set; }
        public object topBanner { get; set; }
        public string listingBanner { get; set; }
    }

    public class Context
    {
        public Prototype prototype { get; set; }
        public object variant { get; set; }
        public string slug { get; set; }
        public string name { get; set; }
        public string boxId { get; set; }
        public string nonce { get; set; }
        public string language { get; set; }
        public bool isDevMode { get; set; }
    }

    public class Prototype
    {
        public string name { get; set; }
        public Version version { get; set; }
    }

    public class Version
    {
        public int major { get; set; }
        public int minor { get; set; }
    }
}