using System;

namespace BankSync.Utilities
{
    public static class BankSyncConverter
    {
        /// <summary>
        /// This is a nasty way of conversion of both 1,5 and 1.5 into 'one and a half'.
        /// It assumes that 1,500 does not mean 1500 etc.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static decimal ConvertWithAssumptions(string input)
        {
            string spaceless = input?.Replace(" ", "")??"";
            try
            {
                return System.Convert.ToDecimal(spaceless);
            }
            catch (Exception)
            {
                if (spaceless.Contains(',') && spaceless.Contains("."))
                {
                    //the last one will be the decimal
                    int indexComma = spaceless.LastIndexOf(',');
                    int indexDot = spaceless.LastIndexOf('.');
                    if (indexDot > indexComma)
                    {
                        return ConvertWithAssumptions(spaceless
                            .Replace(",", "")
                            .Replace(".", ",")
                        );
                    }
                    else
                    {
                        return ConvertWithAssumptions(spaceless
                            .Replace(".","")
                            .Replace(",",".")
                        );
                    }
                }
                else
                {
                    if (spaceless.Contains(","))
                    {
                        return System.Convert.ToDecimal(spaceless.Replace(",","."));
                    }
                    if (spaceless.Contains("."))
                    {
                        return System.Convert.ToDecimal(spaceless.Replace(".",","));
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}