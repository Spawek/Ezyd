using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

//from http://www.codeproject.com/Articles/207602/Creating-a-Bilingual-ASP-NET-MVC3-Application-Part
namespace MvcGlobalisationSupport
{
    public static class CultureFormatChecker
    {
        //This matches the format xx or xx-xx 
        // where x is any alpha character, case insensitive
        //The router will determine if it is a supported language
        static Regex _cultureRegexPattern = new Regex(@"^([a-zA-Z]{2})(-[a-zA-Z]{2})?$", RegexOptions.IgnoreCase & RegexOptions.Compiled);

        public static bool FormattedAsCulture(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;
            return _cultureRegexPattern.IsMatch(code);

        }
    }
}
