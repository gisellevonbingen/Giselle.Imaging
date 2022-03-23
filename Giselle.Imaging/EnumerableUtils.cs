using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public static class EnumerableUtils
    {
        public static IEnumerable<T> TakeElse<T>(this IEnumerable<T> collection, int count, T fallback = default)
        {
            var index = 0;

            foreach (var element in collection)
            {
                if (index < count)
                {
                    yield return element;
                }
                else
                {
                    yield break;
                }

            }

            var remain = count - index;

            for (var i = 0; i < remain; i++)
            {
                yield return fallback;
            }

        }

    }

}
