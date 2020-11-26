using System;
using BankSync.Exporters.Ipko.DataTransformation;
using BankSync.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BankSync.Exporters.Ipko.Tests
{
    [TestClass]
    // ReSharper disable once InconsistentNaming
    public class DescriptionDataExtractor_GetDate_Tests
    {
        private readonly IBankSyncLogger logger = new ConsoleLogger();

        [TestMethod]
        public void DateOnly()
        {
            string input = @"Tytuł: 000498849 74230780303086100480000
Lokalizacja: Kraj: POLSKA Miasto: BYDGOSZCZ Adres: KARAFKA
Data i czas operacji: 2020-10-29
Oryginalna kwota operacji: 61,00 PLN
Numer karty: 005125******0000";

            Check.That(new DescriptionDataExtractor(this.logger).GetDate(input)).IsEqualTo(new DateTime(2020,10,29));
        } 
        
        [TestMethod]
        public void DateAndTime()
        {
            string input = @"Tytuł: 000498849 74230780303086100480000
Lokalizacja: Kraj: POLSKA Miasto: BYDGOSZCZ Adres: KARAFKA
Data i czas operacji: 2020-10-29 11:29:39
Oryginalna kwota operacji: 61,00 PLN
Numer karty: 005125******0000";

            Check.That(new DescriptionDataExtractor(this.logger).GetDate(input)).IsEqualTo(new DateTime(2020,10,29,11,29,39));
        } 
        
    }
}
