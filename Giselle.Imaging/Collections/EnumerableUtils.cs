using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Collections
{
    public static class EnumerableUtils
    {
        public static IEnumerable<T> Range<T>(int fromInclusive, int toExclusive, Func<int, T> func)
        {
            for (var i = fromInclusive; i < toExclusive; i++)
            {
                yield return func(i);
            }

        }

        public static IEnumerable<T> Repeat<T>(int count, Func<int, T> func)
        {
            for (var i = 0; i < count; i++)
            {
                yield return func(i);
            }

        }

        public static IEnumerable<T> TakeFixSize<T>(this IEnumerable<T> source, int offset, int count, T fallback = default)
        {
            var takedCount = 0;

            foreach (var take in source.Skip(offset).Take(count))
            {
                yield return take;
                takedCount++;
            }

            var fallbackCount = count - takedCount;

            for (var i = 0; i < fallbackCount; i++)
            {
                yield return fallback;
            }

        }

        public static IEnumerable<T> TakeFixSize<T>(this IList<T> source, int offset, int count, T fallback = default)
        {
            var takedCount = Math.Min(source.Count - offset, count);

            for (var i = 0; i < takedCount; i++)
            {
                yield return source[offset + i];
            }

            var fallbackCount = count - takedCount;

            for (var i = 0; i < fallbackCount; i++)
            {
                yield return fallback;
            }

        }

    }

}
