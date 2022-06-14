using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Collections
{
    public static class EnumerableUtils
    {
        public static IEnumerable<T> TakeFixSize<T>(this IEnumerable<T> source, int offset, int count, T fallback = default)
        {
            var takes = source.Skip(offset).Take(count).ToArray();

            for (var i = 0; i < takes.Length; i++)
            {
                yield return takes[i];
            }

            var fallbackCount = count - takes.Length;

            for (var i = 0; i < fallbackCount; i++)
            {
                yield return fallback;
            }

        }

        public static IEnumerable<T> TakeFixSize<T>(this IList<T> source, int offset, int count, T fallback = default)
        {
            var takeCount = Math.Max(source.Count - offset, count);

            for (var i = 0; i < takeCount; i++)
            {
                yield return source[offset + i];
            }

            var fallbackCount = count - takeCount;

            for (var i = 0; i < fallbackCount; i++)
            {
                yield return fallback;
            }

        }

    }

}
