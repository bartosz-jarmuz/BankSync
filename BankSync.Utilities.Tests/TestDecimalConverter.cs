using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BankSync.Utilities.Tests
{
    [TestClass]
    public class TestDecimalConverter
    {

        [TestMethod]
        public void Dots()
        {
            BankSyncConverter converter = new BankSyncConverter();
            Check.That(converter.ToDecimal("55.90")).IsEqualTo(55.9M);
        }
        
        [TestMethod]
        public void Commas()
        {
            BankSyncConverter converter = new BankSyncConverter();
            Check.That(converter.ToDecimal("-55,99")).IsEqualTo(-55.99M);
        }
        
        [TestMethod]
        public void Spaces()
        {
            BankSyncConverter converter = new BankSyncConverter();
            Check.That(converter.ToDecimal("1 055,99")).IsEqualTo(1055.99M);
        }

        [TestMethod]
        public void Test()
        {
            BankSyncConverter converter = new BankSyncConverter();
            Check.That(converter.ToDecimal("2.721,55")).IsEqualTo(2721.55M);

            Check.That(converter.ToDecimal("2.721,55")).IsEqualTo(2721.55M);
        }

        [TestMethod]
        public void BigNumbers()
        {
            BankSyncConverter converter = new BankSyncConverter();
            Check.That(converter.ToDecimal("1,055.99")).IsEqualTo(1055.99M);
            
            Check.That(converter.ToDecimal("1.055,99")).IsEqualTo(1055.99M);
            
            Check.That(converter.ToDecimal("-1,000,055.99")).IsEqualTo(-1000055.99M);
            
            Check.That(converter.ToDecimal("1.000.055,99")).IsEqualTo(1000055.99M);
        }
        
        [TestMethod]
        public void Ambiguous_ShouldThrow()
        {
            BankSyncConverter converter = new BankSyncConverter();
            Check.ThatCode(() => converter.ToDecimal("1.000,055,99"))
                .Throws<FormatException>();
            Check.ThatCode(() => converter.ToDecimal("1,000,055,99"))
                .Throws<FormatException>();
            
            Check.ThatCode(() => converter.ToDecimal("1.00.99"))
                .Throws<FormatException>();
        }
    }
}
