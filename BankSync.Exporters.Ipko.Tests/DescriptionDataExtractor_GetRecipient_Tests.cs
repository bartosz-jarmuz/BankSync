using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BankSync.Exporters.Ipko.Tests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class DescriptionDataExtractor_GetRecipient_Tests
    {

        private readonly IBankSyncLogger logger = new ConsoleLogger();

        
        [TestMethod]
        public void FromLocation()
        {
            string input = @"Tytuł: 000498849 74230780303086100485447
Lokalizacja: Kraj: POLSKA Miasto: BYDGOSZCZ Adres: KARAFKA
Data i czas operacji: 2020-10-29
Oryginalna kwota operacji: 61,00 PLN
Numer karty: 425125******1672";

            Check.That(new DescriptionDataExtractor(this.logger).GetRecipient(input)).IsEqualTo("KARAFKA, BYDGOSZCZ");
        } 
        
        [TestMethod]
        public void FromRecipientName()
        {
            string input = @"Rachunek odbiorcy: 28 0000 0022 9000 2016 2222 2222
Nazwa odbiorcy: John DOE
Tytuł: SPŁATA CZĘŚCI ZADŁUŻENIA KARTY KRED YTOWEJ * 0982 OD: 2222222222222
Referencje własne zleceniodawcy: 33333333333333";

            Check.That(new DescriptionDataExtractor(this.logger).GetRecipient(input)).IsEqualTo("John DOE");
        }
        
        [TestMethod]
        public void FromRecipientAccount()
        {
            string input = @"Rachunek odbiorcy: 28 0000 0022 9000 2016 2222 2222
Nazwa odbiorcy: John DOE
Tytuł: SPŁATA CZĘŚCI ZADŁUŻENIA KARTY KRED YTOWEJ * 0982 OD: 2222222222222
Referencje własne zleceniodawcy: 33333333333333";

            Check.That(new DescriptionDataExtractor(this.logger).GetRecipientFromAccount(input)).IsEqualTo("28 0000 0022 9000 2016 2222 2222");
        }
    }
}
