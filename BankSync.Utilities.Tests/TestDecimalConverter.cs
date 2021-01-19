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
            Check.That(BankSyncConverter.ToDecimal("55.90")).IsEqualTo(55.9M);
        }
        
        [TestMethod]
        public void Commas()
        {
            Check.That(BankSyncConverter.ToDecimal("-55,99")).IsEqualTo(-55.99M);
        }
        
        [TestMethod]
        public void Spaces()
        {
            Check.That(BankSyncConverter.ToDecimal("1 055,99")).IsEqualTo(1055.99M);
        }
        
        [TestMethod]
        public void BigNumbers()
        {
            Check.That(BankSyncConverter.ToDecimal("1,055.99")).IsEqualTo(1055.99M);
            
            Check.That(BankSyncConverter.ToDecimal("1.055,99")).IsEqualTo(1055.99M);
            
            Check.That(BankSyncConverter.ToDecimal("-1,000,055.99")).IsEqualTo(-1000055.99M);
            
            Check.That(BankSyncConverter.ToDecimal("1.000.055,99")).IsEqualTo(1000055.99M);
        }
        
        [TestMethod]
        public void Ambiguous_ShouldThrow()
        {
            Check.ThatCode(() => BankSyncConverter.ToDecimal("1.000,055,99"))
                .Throws<FormatException>();
            Check.ThatCode(() => BankSyncConverter.ToDecimal("1,000,055,99"))
                .Throws<FormatException>();
            
            Check.ThatCode(() => BankSyncConverter.ToDecimal("1.00.99"))
                .Throws<FormatException>();
        }
    }
}
