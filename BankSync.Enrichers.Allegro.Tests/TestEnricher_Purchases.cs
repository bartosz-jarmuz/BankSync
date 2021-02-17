using System;
using System.Linq;
using BankSync.Config;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Logging;
using BankSync.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BankSync.Enrichers.Allegro.Tests
{
    [TestClass]
    public class TestEnricher_Purchases
    {
        static IBankSyncLogger log = new ConsoleLogger(); 
        static IDataMapper  map = new DummyMapper();
        private AllegroBankDataEnricher GetEnricher(string fileName) => new AllegroBankDataEnricher(log, new FakeDataLoader(Files.Get(fileName)));
        private BankDataSheet GetData(string fileName) => new IpkoXmlDataTransformer(map ,log).TransformXml(Files.GetXml(fileName));
        
        
        [TestMethod]
        public void SingleOrder_WithDeliveryCost_AllEnriched()
        {
            //arrange
            AllegroBankDataEnricher allegroEnricher = this.GetEnricher(@"Allegro\SingleOrderWithDelivery_Banana.json");
            BankDataSheet data = this.GetData(@"Bank\SingleEntry_Banana.xml");
            
            //act
            allegroEnricher.Enrich(data, DateTime.MinValue, DateTime.MaxValue);

            //assert
            Check.That(data.Entries.Count).IsEqualTo(2);

            BankEntry item = data.Entries.First();
            Check.That(item.Note).IsEqualTo("Banana (Ilość sztuk: 2, Oferta 9465723732, Pozycja 1/1)");
            Check.That(item.Amount).IsEqualTo(-11.98M);
            Check.That(item.Recipient).IsEqualTo("allegro.pl - Seller");
            Check.That(item.Payer).IsEqualTo("PayerPhone");
            
            BankEntry delivery = data.Entries.Last();
            Check.That(delivery.Note).IsEqualTo("DOSTAWA: Banana (Oferta 9465723732, Suma zamówień: 1)");
            Check.That(delivery.Amount).IsEqualTo(-8.99M);
            Check.That(delivery.Recipient).IsEqualTo("allegro.pl - Seller");
            Check.That(delivery.Payer).IsEqualTo("PayerPhone");
        }
        
        [TestMethod]
        public void OlderAndNewerEntriesWithSamePriceAvailable_OnlyTheProperDateEnriched()
        {
            //arrange
            AllegroBankDataEnricher allegroEnricher = this.GetEnricher(@"Allegro\SingleOrderWithDelivery_Banana.json");
            BankDataSheet data = this.GetData(@"Bank\MultipleEntriesSamePriceVariousDates_Banana.xml");
            
            //act
            allegroEnricher.Enrich(data, DateTime.MinValue, DateTime.MaxValue);

            //assert
            Check.That(data.Entries.Count).IsEqualTo(4);

            var matched = data.Entries.Where(x => !x.Note.Contains(AllegroBankDataEnricher.UnrecognizedEntry)).ToList();
            
            
            
            BankEntry item = matched.First();
            Check.That(item.Note).IsEqualTo("Banana (Ilość sztuk: 2, Oferta 9465723732, Pozycja 1/1)");
            Check.That(item.Amount).IsEqualTo(-11.98M);
            Check.That(item.Recipient).IsEqualTo("allegro.pl - Seller");
            Check.That(item.Payer).IsEqualTo("PayerPhone");
            Check.That(item.Date.Date).IsEqualTo(new DateTime(2020,07,27));

            
            BankEntry delivery = matched.Last();
            Check.That(delivery.Note).IsEqualTo("DOSTAWA: Banana (Oferta 9465723732, Suma zamówień: 1)");
            Check.That(delivery.Amount).IsEqualTo(-8.99M);
            Check.That(delivery.Recipient).IsEqualTo("allegro.pl - Seller");
            Check.That(delivery.Payer).IsEqualTo("PayerPhone");
            
            var unmatched = data.Entries.Where(x => x.Note.Contains(AllegroBankDataEnricher.UnrecognizedEntry)).ToList();
            Check.That(unmatched.First().Date.Date).IsEqualTo(new DateTime(2020,09,27));
            Check.That(unmatched.Last().Date.Date).IsEqualTo(new DateTime(2020,02,27));

        }
        
        [TestMethod]
        public void OrdersFromTwoSellers_SinglePayment_Discount_Boots()
        {
            //arrange
            AllegroBankDataEnricher allegroEnricher = this.GetEnricher(@"Allegro\OrdersFromTwoSellers_SinglePayment_Discount_Boots.json");
            BankDataSheet data = this.GetData(@"Bank\NoRefunds_Boots.xml");
            
            //act
            allegroEnricher.Enrich(data, DateTime.MinValue, DateTime.MaxValue);

            //assert
            Check.That(data.Entries.Count).IsEqualTo(2);
            
//            Check.That(data.Entries.Sum(x=>x.Amount)).IsEqualTo(-247.99M);

            BankEntry boots = data.Entries.First();
            Check.That(boots.Note).IsEqualTo("Boots (Ilość sztuk: 1, Oferta 8748604592, Pozycja 1/1)");
            Check.That(boots.Amount).IsEqualTo(-99.99M);
            Check.That(boots.Recipient).IsEqualTo("allegro.pl - SellerOne");
            Check.That(boots.Payer).IsEqualTo("PayerPhone");
            
            BankEntry otherItem = data.Entries.Last();
            Check.That(otherItem.Note).IsEqualTo("Other Item (Ilość sztuk: 1, Oferta 9740390684, Pozycja 1/1)");
            Check.That(otherItem.Amount).IsEqualTo(-158);
            Check.That(otherItem.Recipient).IsEqualTo("allegro.pl - SellerTwo");
            Check.That(otherItem.Payer).IsEqualTo("PayerPhone");
        }
        
        [TestMethod]
        public void OrderFromOneSeller_NoDiscount_DifferentBuyerPaidAmount_Fish()
        {
            //in this scenario, Allegro has applied a different 'buyer paid amount', even though there was no discount,
            //except for delivery costs discount 
            //however, that is normally not included and does not cause differences between the amounts.
            //Weird, but lets make sure it works.
            
            //arrange
            AllegroBankDataEnricher allegroEnricher = this.GetEnricher(@"Allegro\OrderFromOneSeller_NoDiscount_DifferentBuyerPaidAmount_Fish.json");
            BankDataSheet data = this.GetData(@"Bank\NoRefunds_Fish.xml");
            
            //act
            allegroEnricher.Enrich(data, DateTime.MinValue, DateTime.MaxValue);

            //assert
            Check.That(data.Entries.Count).IsEqualTo(2);
            
            Check.That(data.Entries.Sum(x=>x.Amount)).IsEqualTo(-43.77M);

            BankEntry fish = data.Entries.First();
            Check.That(fish.Note).IsEqualTo("Fish (Ilość sztuk: 1, Oferta 8133476102, Pozycja 1/2)");
            Check.That(fish.Amount).IsEqualTo(-21.99M);
            Check.That(fish.Recipient).IsEqualTo("allegro.pl - Seller");
            Check.That(fish.Payer).IsEqualTo("PayerPhone");
            
            BankEntry otherItem = data.Entries.Last();
            Check.That(otherItem.Note).IsEqualTo("Other Item (Ilość sztuk: 2, Oferta 9125409639, Pozycja 2/2)");
            Check.That(otherItem.Amount).IsEqualTo(-21.78M);
            Check.That(otherItem.Recipient).IsEqualTo("allegro.pl - Seller");
            Check.That(otherItem.Payer).IsEqualTo("PayerPhone");
        }
    }
}