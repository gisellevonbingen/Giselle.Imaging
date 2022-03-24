using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public static class ListExtensions
    {
        public static bool StartsWith<T>(this IList<T> list, IList<T> values)
        {
            var count = values.Count;

            if (list.Count < count)
            {
                return false;
            }
            
            for (var i = 0; i < count; i++)
            {
                var o1 = list[i];
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
