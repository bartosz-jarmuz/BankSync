// -----------------------------------------------------------------------
//  <copyright file="DescriptionDataExtractor.cs" >
//   Copyright (c) Bartosz Jarmuz. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;
using BankSync.Exporters.Ipko.Mappers;

namespace BankSync.Exporters.Ipko.DataTransformation
{
    public class DescriptionDataExtractor
    {

        public string GetNote(string description)
        {
            if (description.Contains("Tytuł: "))
            {
                Regex regex = new Regex("(Tytuł: )(.*)");
                Match match = regex.Match(description);
                return match.Groups[2].Value.Trim();

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

        public string GetRecipient(string description)
        {
            if (description.Contains("Nazwa odbiorcy: "))
            {
                Regex regex = new Regex("(Nazwa odbiorcy: )(.*)");
                Match match = regex.Match(description);
                return match.Groups[2].Value.Trim();
            }
            else if (description.Contains("Lokalizacja: "))
            {
                try
                {
                    string city = GetCity(description);
                    string address = GetAddress(description);

                    string value = $"{address}";
                    if (!string.IsNullOrEmpty(city))
                    {
                        value += $", {city}";
                    }

                    return value;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return description;
                }
            }
            else
            {
                return "";
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