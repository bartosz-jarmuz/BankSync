using System;
using System.Globalization;
using System.Threading;

namespace BankSync.Utilities
{
    public class BankSyncConverter
    {

        /// <summary>
        /// This is a nasty way of conversion of both 1,5 and 1.5 into 'one and a half'.
        /// It assumes that 1,500 does not mean 1500 etc.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public decimal ToDecimal(string input)
        {
            string spaceless = input?.Replace(" ", "") ?? "";

            if (Decimal.TryParse(spaceless, out decimal converted))
            {
                return converted;
            }
            else
            {
                if (spaceless.Contains(',') && spaceless.Contains("."))
                {
                    //the last one will be the decimal
                    int indexComma = spaceless.LastIndexOf(',');
                    int indexDot = spaceless.LastIndexOf('.');
                    if (indexDot > indexComma)
                    {
                        return ToDecimal(spaceless
                            .Replace(",", "")
                            .Replace(".", ",")
                        );
                    }
                    else
                    {
                        return ToDecimal(spaceless
                            .Replace(".", "")
                            .Replace(",", ".")
                        );
                    }
                }
                else
                {
                    if (spaceless.Contains(","))
                    {
                        return System.Convert.ToDecimal(spaceless.Replace(",", "."));
                    }

                    if (spaceless.Contains("."))
                    {
                        return System.Convert.ToDecimal(spaceless.Replace(".", ","));
                    }
                    else
                    {
                        throw new FormatException($"Unexpected format of input string: {spaceless}");
                    }
                }
            }
        }


    }
}