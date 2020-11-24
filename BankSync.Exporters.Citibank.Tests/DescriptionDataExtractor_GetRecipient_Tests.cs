using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BankSync.Exporters.Citibank.Tests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class DescriptionDataExtractor_GetRecipient_Tests
    {
        [TestMethod]
        public void FromDescription_SpaceDelimited()
        {
            Check.That(new DescriptionDataExtractor().GetRecipient("PayU*Allegro             Poznan       PL")).IsEqualTo("PayU*Allegro");
            
            Check.That(new DescriptionDataExtractor().GetRecipient("Ticketmaster Poland      Warszawa     PL")).IsEqualTo("Ticketmaster Poland");
            
            Check.That(new DescriptionDataExtractor().GetRecipient("Leroy Merlin Bydgoszcz   Bydgoszcz    PL      Warszawa     PL")).IsEqualTo("Leroy Merlin Bydgoszcz");
            
            Check.That(new DescriptionDataExtractor().GetRecipient("JMP S.A. BIEDRONKA 505   ZLAWIES WIELKPL")).IsEqualTo("JMP S.A. BIEDRONKA 505");
        } 
        
        [TestMethod]
        public void FromDescription_Slashes()
        {
            Check.That(new DescriptionDataExtractor().GetRecipient("BOLT.EU /R/2010041413    Tallinn      EE")).IsEqualTo("BOLT.EU");
            Check.That(new DescriptionDataExtractor().GetRecipient("APPLE.COM/BILL           008004411875 IE")).IsEqualTo("BOLT.EU");

        }
    }
}
