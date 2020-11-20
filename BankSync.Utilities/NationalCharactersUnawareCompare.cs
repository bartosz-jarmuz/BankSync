using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace BankSync.Utilities
{
    public static class NationalCharactersUnawareCompare
    {
        public static bool AreEqual(string left, string right)
        {
            if (left == right)
            {
                return true;
            }

            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
            {
                return false;
            }

            var leftClean = RemovePolishNationalCharacters(left.ToLowerInvariant());
            var rightClean= RemovePolishNationalCharacters(right.ToLowerInvariant());

            return String.Equals(leftClean, rightClean, StringComparison.OrdinalIgnoreCase);

        }


        public static bool ContainsNationalUnaware(this string left, string right)
        {
            if (left == right || left.Contains(right, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right))
            {
                return false;
            }

            var leftClean = RemovePolishNationalCharacters(left.ToLowerInvariant());
            var rightClean = RemovePolishNationalCharacters(right.ToLowerInvariant());

            return leftClean.Contains(rightClean, StringComparison.OrdinalIgnoreCase);

        }

        private static string RemovePolishNationalCharacters(string input)
        {
            return input
                    .Replace("ą", "a")
                    .Replace("ę", "e")
                    .Replace("ś", "s")
                    .Replace("ć", "c")
                    .Replace("ź", "z")
                    .Replace("ż", "z")
                    .Replace("ó", "o")
                    .Replace("ł", "l")
                    .Replace("ń", "n")
                ;
        }
    }
}
