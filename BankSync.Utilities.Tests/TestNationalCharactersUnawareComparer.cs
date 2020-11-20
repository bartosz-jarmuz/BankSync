using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BankSync.Utilities.Tests
{
    [TestClass]
    public class TestNationalCharactersUnawareComparer
    {

        [TestMethod]
        public void WhenStringIsEquivalent_ReturnsTrue()
        {
            string left = "ęóąśłŻźćń";
            string right = "Eoaslzzcn";

            Check.That(NationalCharactersUnawareCompare.AreEqual(left, right)).IsTrue();
        }

        [TestMethod]
        public void WhenStringIsDifferent_ReturnsFalse()
        {
            string left = "left";
            string right = "le ft";
            
            Check.That(NationalCharactersUnawareCompare.AreEqual(left, right)).IsFalse();
        }

        [TestMethod]
        public void WhenStringIsContained_ReturnsTrue()
        {
            string left = "foo ęóąśłŻ źćń bar";
            string right = "Eoaslz zcn";

            Check.That(left.ContainsNationalUnaware(right)).IsTrue();
        }

        [TestMethod]
        public void WhenStringIsNotContained_ReturnsFalse()
        {
            string left = "left ri";
            string right = "right";

            Check.That(left.ContainsNationalUnaware(right)).IsFalse();
        }
    }
}
