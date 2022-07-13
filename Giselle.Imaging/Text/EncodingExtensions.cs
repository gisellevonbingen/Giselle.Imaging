using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Text
{
    public static class EncodingExtensions
    {
        public static string GetStringUntilNull(this Encoding encoding, byte[] bytes) => GetStringUntilNull(encoding, bytes, 0, bytes.Length);

        public static string GetStringUntilNull(this Encoding encoding, byte[] bytes, int offset, int count)
        {
            var nullIndex = Array.IndexOf(bytes, byte.MinValue, offset);
            return encoding.GetString(bytes, offset, nullIndex > -1 ? nullIndex : count);
        }

    }

}
