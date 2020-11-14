using Microsoft.VisualStudio.TestTools.UnitTesting;
using NFluent;

namespace BankSync.Utilities.Tests
{
    [TestClass]
    public class TestMaskedInputRecognizer
    {

        [TestMethod]
        public void WhenNotMasked_Equal_IsMatchedTrue()
        {
            string masked = "aa1234";
            string unmasked = "aa1234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsTrue();
            Check.That(MaskedInputRecognizer.IsMatch(null, null)).IsTrue();
            Check.That(MaskedInputRecognizer.IsMatch("","")).IsTrue();
        }

        [TestMethod]
        public void WhenNotMasked_Different_IsMatchedFalse()
        {
            string masked = "AA1234";
            string unmasked = "aa1234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsFalse();
        }

        [TestMethod]
        public void WhenOneIsNullOrEmpty_IsMatchedFalse()
        {
            string masked = "";
            string unmasked = "aa1234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsFalse();
            Check.That(MaskedInputRecognizer.IsMatch(masked, null)).IsFalse();
            Check.That(MaskedInputRecognizer.IsMatch(null, unmasked)).IsFalse();
            Check.That(MaskedInputRecognizer.IsMatch(null, "")).IsFalse();
        }

        [TestMethod]
        public void NoSpaces_WhenBeginningEndAndLengthMatch_IsMatchedTrue()
        {
            string masked = "aa**********1234";
            string unmasked = "aa12345678901234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsTrue();
        }

        [TestMethod]
        public void NoSpaces_WhenBeginningEndMatch_ButNotLength_IsMatchedFalse()
        {
            string masked = "aa*******1234";
            string unmasked = "aa12345678901234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsFalse();
        }

        [TestMethod]
        public void NoSpaces_WhenEndAndLengthMatch_ButNotBeginning_IsMatchedFalse()
        {
            string masked = "aa**********1234";
            string unmasked = "bb12345678901234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsFalse();
        }

        [TestMethod]
        public void NoSpaces_WhenEndAndLengthMatch_ButNotBeginningCase_IsMatchedFalse()
        {
            string masked = "aa**********1234";
            string unmasked = "AA12345678901234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsFalse();
        }

        [TestMethod]
        public void NoSpaces_WhenBeginningAndLengthMatch_ButNotEnd_IsMatchedFalse()
        {
            string masked = "aa**********6666";
            string unmasked = "aa12345678901234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsFalse();
        }

        [TestMethod]
        public void NoSpaces_WhenBeginningAndLengthMatch_ButNotEndCase_IsMatchedFalse()
        {
            string masked = "44**********aa";
            string unmasked = "441234567890AA";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsFalse();
        }

        [TestMethod]
        public void WithSpacesInMask_WhenBeginningEndAndLengthMatch_IsMatchedTrue()
        {
            string masked = "aa ***** ***** 1234";
            string unmasked = "aa12345678901234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsTrue();
        }

        [TestMethod]
        public void WithSpacesInMask_BothAreMasked_WhenBeginningEndAndLengthMatch_IsMatchedTrue()
        {
            string masked = "aa ***** ***** 1234";
            string unmasked = "aa ** ** ** ** **1234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsTrue();
        }

        [TestMethod]
        public void WithSpacesInActual_WhenBeginningEndAndLengthMatch_IsMatchedTrue()
        {
            string masked = "aa**********1234";
            string unmasked = "aa 12345 67890 1234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsTrue();
        }

        [TestMethod]
        public void WithSpacesInBoth_WhenBeginningEndAndLengthMatch_IsMatchedTrue()
        {
            string masked = "aa* * *** *** **1234";
            string unmasked = "aa 12 345 67890 1234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsTrue();
        }

        [TestMethod]
        public void WithSpacesInMask_WhenBeginningEndMatch_ButNotLength_IsMatchedFalse()
        {
            string masked = "aa ***** *** 1234";
            string unmasked = "aa12345678901234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsFalse();
        }

        [TestMethod]
        public void WithSpacesInRevealedParts_WhenBeginningEndMatch_ButNotLength_IsMatchedFalse()
        {
            string masked = "aa* * *** *** **12 34";
            string unmasked = "a a 12 345 67890 1234";

            Check.That(MaskedInputRecognizer.IsMatch(masked, unmasked)).IsTrue();
        }
    }
}
