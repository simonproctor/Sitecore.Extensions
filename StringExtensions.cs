using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.Extensions
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string s)
        {
            var ti = System.Globalization.CultureInfo.InvariantCulture.TextInfo;
            return ti.ToTitleCase(s);
        }
    }
}
