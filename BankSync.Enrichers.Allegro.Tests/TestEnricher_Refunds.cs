using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankSync.Config;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Logging;
using BankSync.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BankSync.Enrichers.Allegro.Tests
{
    [TestClass]
    public class TestEnricherRefunds
    {
        private static readonly IBankSyncLogger Log = new ConsoleLogger();
        private static readonly IDataMapper  Map = new DummyMapper();
        private AllegroBankDataEnricher GetEnricher(string fileName) => new AllegroBankDataEnricher(Log, new FakeDataLoader(Files.Get(fileName)));
        private BankDataSheet GetData(string fileName) => new IpkoXmlDataTransformer(Map ,Log).TransformXml(Files.GetXml(fileName));
        
        
        [TestMethod]
        public void MultiOrder_NoRefunds_AllEnriched()
        {
            //arrange
            AllegroBankDataEnricher allegroEnricher = this.GetEnricher(@"Allegro\MultiOrderWithoutDelivery_Prunes.json");
            BankDataSheet data = this.GetData(@"Bank\NoRefunds_Prunes.xml");
            
            //act
            allegroEnricher.Enrich(data, DateTime.MinValue, DateTime.MaxValue);

            //assert
            Check.That(data.Entries.Count).IsEqualTo(5);

            Check.That(data.Entries.Sum(x => x.Amount)).IsEqualTo(-30.94);

            foreach (BankEntry dataEntry in data.Entries)
            {
                Check.That(dataEntry.Recipient).IsEqualTo("allegro.pl - Seller");
                Check.That(dataEntry.Payer).IsEqualTo("PayerPhone");
                Check.That(dataEntry.Note).Contains("Ilość sztuk:");
            }
            
            Check.That(data.Entries.Count(x => x.Amount == -3.45M)).IsEqualTo(3);
            Check.That(data.Entries.Count(x => x.Amount == -6.90M)).IsEqualTo(1);
            Check.That(data.Entries.Count(x => x.Amount == -13.69M)).IsEqualTo(1);
        }
        
        [TestMethod]
        public async Task MultiOrder_FullRefund_AllEnriched()
        {
            //arrange
            AllegroBankDataEnricher allegroEnricher = this.GetEnricher(@"Allegro\MultiOrderWithoutDelivery_Prunes.json");
            BankDataSheet data = this.GetData(@"Bank\FullRefund_Prunes.xml");

            //act
            await allegroEnricher.Enrich(data, DateTime.MinValue, DateTime.MaxValue);

            //assert
            Check.That(data.Entries.Count).IsEqualTo(11);

            Check.That(data.Entries.Sum(x => x.Amount)).IsEqualTo(30.94);

            List<BankEntry> negative = data.Entries.Where(x => x.Amount < 0).ToList();
            
            foreach (BankEntry dataEntry in negative)
            {
                Check.That(dataEntry.Recipient).IsEqualTo("allegro.pl - Seller");
                Check.That(dataEntry.Payer).IsEqualTo("PayerPhone");
                Check.That(dataEntry.Note).Contains("Ilość sztuk:");
            }
            
            Check.That(negative.Count(x => x.Amount == -3.45M)).IsEqualTo(3);
            Check.That(negative.Count(x => x.Amount == -6.90M)).IsEqualTo(1);
            Check.That(negative.Count(x => x.Amount == -13.69M)).IsEqualTo(1);

            BankEntry tooOldRefund = data.Entries.Single(x => x.Amount > 0 && x.FullDetails.Contains("TOO OLD"));
            Check.That(tooOldRefund.Note).IsEqualTo("Nierozpoznany zwrot");
            Check.That(tooOldRefund.Payer).IsEqualTo("allegro.pl (Nierozpoznany sprzedawca)");
            Check.That(tooOldRefund.Recipient).IsEqualTo("Nierozpoznany odbiorca");
            Check.That(tooOldRefund.Amount).IsEqualTo(30.94M);
            
            
            List<BankEntry> positive = data.Entries.Where(x => x.Amount > 0 && !x.FullDetails.Contains("TOO OLD")).ToList();
            Check.That(positive.Count).IsEqualTo(5);
            foreach (BankEntry dataEntry in positive)
            {
                Check.That(dataEntry.Recipient).IsEqualTo("Bartek");
                Check.That(dataEntry.Payer).IsEqualTo("allegro.pl - Seller");
                Check.That(dataEntry.Note).Contains("Ilość sztuk:");
                Check.That(dataEntry.FullDetails).DoesNotContain("TOO NEW").And.DoesNotContain("TOO OLD").And.Contains("JUST RIGHT REFUND");
            }
            
            Check.That(positive.Count(x => x.Amount == 3.45M)).IsEqualTo(3);
            Check.That(positive.Count(x => x.Amount == 6.90M)).IsEqualTo(1);
            Check.That(positive.Count(x => x.Amount == 13.69M)).IsEqualTo(1);
        }
        
        
         [TestMethod]
        public async Task MultiOrderFromVariousSellers_PartialRefund()
        {
            //arrange
            AllegroBankDataEnricher allegroEnricher = this.GetEnricher(@"Allegro\OrdersFromTwoSellers_SinglePayment_Discount_Boots.json");
            BankDataSheet data = this.GetData(@"Bank\PartialRefund_Boots.xml");

            //act
            await allegroEnricher.Enrich(data, DateTime.MinValue, DateTime.MaxValue);

            //assert
            Check.That(data.Entries.Count).IsEqualTo(3);

            Check.That(data.Entries.Sum(x => x.Amount)).IsEqualTo(-158);

            var refund = data.Entries.Single(x => x.Amount > 0);

            Check.That(refund.Amount).IsEqualTo(99.99M);
            Check.That(refund.Recipient).IsEqualTo("Bartek");
            Check.That(refund.Payer).IsEqualTo("allegro.pl - SellerOne");
            Check.That(refund.Note).Contains("Boots (Ilość sztuk:");
            
            List<BankEntry> negative = data.Entries.Where(x => x.Amount < 0).ToList();
            BankEntry boots = negative.First();
            Check.That(boots.Note).IsEqualTo("Boots (Ilość sztuk: 1, Oferta 8748604592, Pozycja 1/1)");
            Check.That(boots.Amount).IsEqualTo(-99.99M);
            Check.That(boots.Recipient).IsEqualTo("allegro.pl - SellerOne");
            Check.That(boots.Payer).IsEqualTo("PayerPhone");
            
            BankEntry otherItem = negative.Last();
            Check.That(otherItem.Note).IsEqualTo("Other Item (Ilość sztuk: 1, Oferta 9740390684, Pozycja 1/1)");
            Check.That(otherItem.Amount).IsEqualTo(-158);
            Check.That(otherItem.Recipient).IsEqualTo("allegro.pl - SellerTwo");
            Check.That(otherItem.Payer).IsEqualTo("PayerPhone");
            
           
        }
        
        [TestMethod]
        public void RefundOfOne_ByPrice_Prunes()
        {
            //arrange
            AllegroBankDataEnricher allegroEnricher = this.GetEnricher(@"Allegro\MultiOrderWithoutDelivery_Prunes.json");
            BankDataSheet data = this.GetData(@"Bank\RefundOfOne_ByPrice_Prunes.xml");
            
            //act
            allegroEnricher.Enrich(data, DateTime.MinValue, DateTime.MaxValue);

            //assert
            Check.That(data.Entries.Count).IsEqualTo(6);

            Check.That(data.Entries.Sum(x => x.Amount)).IsEqualTo(-27.49M);

            List<BankEntry> negative = data.Entries.Where(x => x.Amount < 0).ToList();
            
            foreach (BankEntry dataEntry in negative)
            {
                Check.That(dataEntry.Recipient).IsEqualTo("allegro.pl - Seller");
                Check.That(dataEntry.Payer).IsEqualTo("PayerPhone");
                Check.That(dataEntry.Note).Contains("Ilość sztuk:");
            }
            
            Check.That(negative.Count(x => x.Amount == -3.45M)).IsEqualTo(3);
            Check.That(negative.Count(x => x.Amount == -6.90M)).IsEqualTo(1);
            Check.That(negative.Count(x => x.Amount == -13.69M)).IsEqualTo(1);
            
            
            var positive = data.Entries.Single(x => x.Amount > 0);
            
            Check.That(positive.Amount).IsEqualTo(3.45M);
           
        }
        
        [TestMethod]
        public void RefundOfTwo_PriceDoesNotMatch_Prunes()
        {
            //arrange
            AllegroBankDataEnricher allegroEnricher = this.GetEnricher(@"Allegro\MultiOrderWithoutDelivery_Prunes.json");
            BankDataSheet data = this.GetData(@"Bank\RefundOfTwo_PriceDoesNotMatch_Prunes.xml");
            
            //act
            allegroEnricher.Enrich(data, DateTime.MinValue, DateTime.MaxValue);

            //assert
            Check.That(data.Entries.Count).IsEqualTo(6);

            Check.That(data.Entries.Sum(x => x.Amount)).IsEqualTo(-13.80M);

            List<BankEntry> negative = data.Entries.Where(x => x.Amount < 0).ToList();
            
            foreach (BankEntry dataEntry in negative)
            {
                Check.That(dataEntry.Recipient).IsEqualTo("allegro.pl - Seller");
                Check.That(dataEntry.Payer).IsEqualTo("PayerPhone");
                Check.That(dataEntry.Note).Contains("Ilość sztuk:");
            }
            
            Check.That(negative.Count(x => x.Amount == -3.45M)).IsEqualTo(3);
            Check.That(negative.Count(x => x.Amount == -6.90M)).IsEqualTo(1);
            Check.That(negative.Count(x => x.Amount == -13.69M)).IsEqualTo(1);
            
            
            BankEntry positive = data.Entries.Single(x => x.Amount > 0);
            
            Check.That(positive.Amount).IsEqualTo(17.14M);
            Check.That(positive.Note).IsEqualTo("Nierozpoznany zwrot");
            
           
        }
    }
}