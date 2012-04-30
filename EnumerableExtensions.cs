using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sitecore.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list)
        {
            if (list == null)
                return true;

            var genericCollection = list as ICollection<T>;
            if (genericCollection != null)
            {
                return genericCollection.Count == 0;
            }

            var nonGenericCollection = list as ICollection;
            if (nonGenericCollection != null)
            {
                return nonGenericCollection.Count == 0;
            }

            return !list.Any();
        }

        public static string JoinStrings(this IEnumerable<string> str, string separator)
        {
            if (str.IsNullOrEmpty()) return String.Empty;

            var arr = str.ToArray();
            return String.Join(separator, arr);
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }
    }
}
