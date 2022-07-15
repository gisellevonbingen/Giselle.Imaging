using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Collections
{
    public static class ListExtensions
    {
        public static bool StartsWith<T>(this IList<T> list, IList<T> values) => StartsWith(list, values, 0);

        public static bool StartsWith<T>(this IList<T> list, IList<T> values, int offset)
        {
            var count = values.Count;

            if (list.Count + offset < count)
            {
                return false;
            }

            for (var i = 0; i < count; i++)
            {
                var o1 = list[offset + i];
                var o2 = values[i];

                if (ObjectUtils.Equals(o1, o2) == false)
                {
                    return false;
                }

            }

            return true;
        }

    }

}
