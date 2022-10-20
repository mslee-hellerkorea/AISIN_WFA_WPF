using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AISIN_WFA.Model
{
    public class RegexLib
    {
        public static bool IsValidCurrency(string currencyValue, string pattern)
        {
            string value = currencyValue.Replace("\0", "");
            string ptn = WildCardToRegular(pattern);
            bool result = Regex.IsMatch(value, ptn);
            return result;
        }

        private static string WildCardToRegular(string value)
        {
            //return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".") + "$";
        }
    }
}
