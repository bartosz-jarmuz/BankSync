using System.Linq;
using System.Text.RegularExpressions;

namespace BankSync.Utilities
{
    public static class MaskedInputRecognizer
    {
        public static bool IsMatch(string masked, string potentialMatch)
        {
            if (masked == potentialMatch)
            {
                return true;
            }

            if (string.IsNullOrEmpty(masked) || string.IsNullOrEmpty(potentialMatch))
            {
                return false;
            }
            masked = masked.Replace(" ", "");
            potentialMatch = potentialMatch.Replace(" ", "");
            string beginning = Regex.Match(masked, @"^[^*]*")?.Value;
            int numberOfStars = masked.Count(c => c == '*');
            string end = Regex.Match(masked, @"[^*]*$")?.Value;

            if (potentialMatch.StartsWith(beginning) && potentialMatch.EndsWith(end))
            {
                if (potentialMatch.Length == beginning?.Length + numberOfStars + end?.Length)
                {
                    return true;
                }
            }

            return false;

        }
    }
}
