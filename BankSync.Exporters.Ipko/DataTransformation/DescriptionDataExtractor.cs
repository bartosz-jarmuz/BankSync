// -----------------------------------------------------------------------
//  <copyright file="DescriptionDataExtractor.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using BankSync.Exporters.Ipko.Mappers;

namespace BankSync.Exporters.Ipko.DataTransformation
{
    public class DescriptionDataExtractor
    {
        public string GetPayer(string description)
        {
            string value;
            if (description.StartsWith("Numer telefonu: "))
            {
                string part = description.Substring("Numer telefonu: ".Length);
                value = part.Remove(part.IndexOf("Lokalizacja", StringComparison.OrdinalIgnoreCase)).Trim();
            }
            else if (description.Contains("Numer karty: "))
            {
                value = description.Substring(description.IndexOf("Numer karty: ", StringComparison.OrdinalIgnoreCase) + "Numer karty: ".Length);
            }
            else if (description.Contains("Nazwa nadawcy: "))
            {
                Regex regex = new Regex("(Nazwa nadawcy: )(.*)");
                Match match = regex.Match(description);
                value = match.Groups[2].Value.Trim();
            }
            else
            {
                value = "";
            }

            return value;
        }
    }
}