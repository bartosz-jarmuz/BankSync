﻿// -----------------------------------------------------------------------
//  <copyright file="DescriptionDataExtractor.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace BankSync.Exporters.Citibank
{
    public class DescriptionDataExtractor
    {

        public string GetNote(string description)
        {
            if (description.Contains("Tytuł: "))
            {
                Regex regex = new Regex("(Tytuł: )(.*)");
                Match match = regex.Match(description);

                string value = match.Groups[2].Value.Trim();

                if (Regex.IsMatch(value, @"^[\d ]+$"))
                {
                    return "";
                }

                return value;

            }
            else
            {
                return description;
            }

        }

        public string GetPayer(string description)
        {
            if (description.StartsWith("Numer telefonu: "))
            {
                string part = description.Substring("Numer telefonu: ".Length);
                return part.Remove(part.IndexOf("Lokalizacja", StringComparison.OrdinalIgnoreCase)).Trim();
            }
            else if (description.Contains("Numer karty: "))
            {
                return description.Substring(description.IndexOf("Numer karty: ", StringComparison.OrdinalIgnoreCase) + "Numer karty: ".Length);
            }
            else if (description.Contains("Nazwa nadawcy: "))
            {
                Regex regex = new Regex("(Nazwa nadawcy: )(.*)");
                Match match = regex.Match(description);
                return match.Groups[2].Value.Trim();
            }
            else
            {
                return "";
            }
        }
        
        /// <summary>
        /// When you own multiple accounts, your better bet at figuring out tha payer is by checking the account number
        /// because the 'nazwa nadawcy' will be same person for multiple sources
        /// However, when you receive money from a stranger, you better figure out who its from by the name, not a number
        /// Therefore, a quick win is to try mapping account number, and if not, then go with the regular flow
        /// </summary>
        /// <param name="description"></param>
        /// <returns></returns>
        public string GetPayerFromAccount(string description)
        {
            if (description.StartsWith("Rachunek nadawcy: "))
            {
                string part = description.Substring("Rachunek nadawcy: ".Length);
                return part.Remove(part.IndexOf("Nazwa ", StringComparison.OrdinalIgnoreCase)).Trim();
            }

            return null;
        }

        public string GetRecipient(string description)
        {
            if (description.Contains("/"))
            {
                return description.Remove(description.IndexOf("/"));
            }
            else if (description.Contains("  "))
            {
                return description.Remove(description.IndexOf("  ")).Trim();
            }
            else
            {
                return description;
            }
        }

        private static string GetAddress(string description)
        {
            string address = description.Substring(description.IndexOf("Adres:", StringComparison.Ordinal) + "Adres:".Length);
            address = address.Remove(address.IndexOf("Data i czas operacji:", StringComparison.Ordinal)).Trim();
            return address;
        }

        private static string GetCity(string description)
        {
            if (description.IndexOf("Miasto: ", StringComparison.Ordinal) == -1)
            {
                return "";
            }
            else
            {
                string part = description.Substring(description.IndexOf("Miasto: ", StringComparison.Ordinal));
                string city = part.Substring("Miasto: ".Length);

                city = city.Remove(city.IndexOf("Adres:", StringComparison.Ordinal)).Trim();
                return city;
            }
        }
    }
}