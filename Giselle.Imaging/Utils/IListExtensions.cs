using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Utils
{
    public static class IListExtensions
    {
        public static bool TestIndex<T>(this IList<T> list, int index) => 0 <= index && index < list.Count;

        public static bool TryGet<T>(this IList<T> list, int index, out T value)
        {
            if (TestIndex(list, index) == true)
            {
                value = list[index];
                return true;
            }
            else
            {
                value = default;
                return false;
            }

        }

    }

}
