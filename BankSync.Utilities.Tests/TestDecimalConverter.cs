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
            Check.That(BankSyncConverter.ConvertWithAssumptions("55.90")).IsEqualTo(55.9M);
        }
        
        [TestMethod]
        public void Commas()
        {
            Check.That(BankSyncConverter.ConvertWithAssumptions("-55,99")).IsEqualTo(-55.99M);
        }
        
        [TestMethod]
        public void Spaces()
        {
            Check.That(BankSyncConverter.ConvertWithAssumptions("1 055,99")).IsEqualTo(1055.99M);
        }
        
        [TestMethod]
        public void BigNumbers()
        {
            Check.That(BankSyncConverter.ConvertWithAssumptions("1,055.99")).IsEqualTo(1055.99M);
            
            Check.That(BankSyncConverter.ConvertWithAssumptions("1.055,99")).IsEqualTo(1055.99M);
            
            Check.That(BankSyncConverter.ConvertWithAssumptions("-1,000,055.99")).IsEqualTo(-1000055.99M);
            
            Check.That(BankSyncConverter.ConvertWithAssumptions("1.000.055,99")).IsEqualTo(1000055.99M);
        }
        
        [TestMethod]
        public void Ambiguous_ShouldThrow()
        {
            Check.ThatCode(() => BankSyncConverter.ConvertWithAssumptions("1.000,055,99"))
                .Throws<FormatException>();
            Check.ThatCode(() => BankSyncConverter.ConvertWithAssumptions("1,000,055,99"))
                .Throws<FormatException>();
            
            Check.ThatCode(() => BankSyncConverter.ConvertWithAssumptions("1.00.99"))
                .Throws<FormatException>();
        }
    }
}
