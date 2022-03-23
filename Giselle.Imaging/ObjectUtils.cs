using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public static class ObjectUtils
    {
        public static bool Equals<T>(T o1, T o2)
        {
            if (o1 is ValueType)
            {
                if (o2 is ValueType)
                {
                    return o1.Equals(o2);
                }
                else
                {
                    return false;
                }

            }
            else if (o2 is ValueType)
            {
                return false;
            }
            else if (o1 == null)
            {
                return o2 == null;
            }
            else if (o2 == null)
            {
                return false;
            }

            return o1.Equals(o2);
        }

    }

}
