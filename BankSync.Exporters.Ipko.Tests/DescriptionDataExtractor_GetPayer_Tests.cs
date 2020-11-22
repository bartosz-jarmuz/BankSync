using BankSync.Exporters.Ipko.DataTransformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BankSync.Exporters.Ipko.Tests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class DescriptionDataExtractor_GetPayer_Tests
    {
        [TestMethod]
        public void FromPhoneNumber()
        {
            string input = @"Numer telefonu: +48 725 795 221 
Lokalizacja: Adres: https://pomagam.pl
Data i czas operacji: 2020-07-31 18:46:11
Numer referencyjny: 00000053305951531";

            Check.That(new DescriptionDataExtractor().GetPayer(input)).IsEqualTo("+48 725 795 221");
        } 
        
        [TestMethod]
        public void FromCard()
        {
            string input = @"Tytuł: 000483849 74838490213248803395148
Lokalizacja: Kraj: POLSKA Miasto: BYDGOSZCZ Adres: ROSSMANN 24
Data i czas operacji: 2020-07-31 16:10:58
Oryginalna kwota operacji: 81,55 PLN
Numer karty: 425125******2222";

            Check.That(new DescriptionDataExtractor().GetPayer(input)).IsEqualTo("425125******2222");
        }
        
        [TestMethod]
        public void FromAccountName()
        {
            string input = @"Rachunek nadawcy: 22 0000 1506 0000 0001 0700 2222
Nazwa nadawcy: Pani JOHN DOE UL. ZŁA 666 87-134 CZARNOWO
Tytuł: Kasa";

            Check.That(new DescriptionDataExtractor().GetPayer(input)).IsEqualTo("Pani JOHN DOE UL. ZŁA 666 87-134 CZARNOWO");


            input = @"Rachunek nadawcy: 22 0000 1475 0000 8802 0257 8888
Nazwa nadawcy: JIM N BEAM
Adres nadawcy: UL. 22 Accacia Avenue
Tytuł: WYCIAG
Referencje własne zleceniodawcy: 172173609222";

            Check.That(new DescriptionDataExtractor().GetPayer(input)).IsEqualTo("JIM N BEAM");

        } 
        
        [TestMethod]
        public void FromAccountNumber()
        {
            string input = @"Rachunek nadawcy: 22 0000 1506 0000 0001 0700 2222
Nazwa nadawcy: Pani JOHN DOE UL. ZŁA 666 87-134 CZARNOWO
Tytuł: Kasa";

            Check.That(new DescriptionDataExtractor().GetPayerFromAccount(input)).IsEqualTo("22 0000 1506 0000 0001 0700 2222");


            

        }
    }
}
