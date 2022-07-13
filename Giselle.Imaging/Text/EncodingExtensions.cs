using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;

namespace Giselle.Imaging.Text
{
    public static class EncodingExtensions
    {
        public static string GetStringUntil0(this Encoding encoding, byte[] bytes) => GetStringUntil0(encoding, bytes, 0, bytes.Length);

        public static string GetStringUntil0(this Encoding encoding, byte[] bytes, int offset, int count)
        {
            var nullIndex = Array.IndexOf(bytes, byte.MinValue, offset);
            return encoding.GetString(bytes, offset, nullIndex > -1 ? nullIndex : count);
        }

        public static byte[] GetBytesWith0(this Encoding encoding, string s, int byteCount)
        {
            var raw = encoding.GetBytes(s);
            var pack = new byte[byteCount + 1];
            Array.Copy(raw, 0, pack, 0, Math.Min(raw.Length, byteCount));
            return pack;
        }

    }

}
