using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ezyd.Models.stringHelper
{
    public static class BadCharsRemover
    {
        public static void RemoveBadChars(ref string str)
        {
            str = str.Replace('\'', ' ')
                .Replace('"', ' ')
                .Replace('`', ' ')
                .Replace(';', ' ')
                .Replace('@', ' ')
                .Replace("--", "-")
                .Replace(";--", string.Empty)
                .Replace("/*", string.Empty)
                .Replace("*/", string.Empty);
        }
    }
}