using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;
using Giselle.Imaging.Text;

namespace Giselle.Imaging.Utils
{
    public static class BitConverter2
    {
        public static Encoding DefaultEncoding => Encoding.ASCII;

        public static (byte Upper, byte Lower) SplitNibbles(byte value)
        {
            var upper = (byte)((value >> 0x04) & 0x0F);
            var lower = (byte)((value >> 0x00) & 0x0F);
            return (upper, lower);
        }

        public static int ToASCIIInt32(this string value) => ToASCIIInt32(value, BitConverter.IsLittleEndian);

        public static int ToASCIIInt32(this string value, bool isLittleEndian) => ToASCIINumber(value, 4, isLittleEndian, p => p.ReadInt());

        public static T ToASCIINumber<T>(this string value, int size, bool isLittleEndian, Func<DataProcessor, T> func)
        {
            if (string.IsNullOrEmpty(value) == true)
            {
                return default;
            }

            using (var ms = new MemoryStream())
            {
                var processor = new DataProcessor(ms) { IsLittleEndian = isLittleEndian };
                processor.WriteBytes(Encoding.ASCII.GetBytes(value));

                var missings = size - processor.Length;

                if (missings > 0)
                {
                    for (var i = 0; i < missings; i++)
                    {
                        processor.WriteByte(0);
                    }

                }

                ms.Position = 0L;
                return func(processor);
            }

        }

        public static string ToASCIIString(this int value) => ToASCIIString(value, BitConverter.IsLittleEndian);

        public static string ToASCIIString(this int value, bool isLittleEndian) => ToASCIIString(isLittleEndian, p => p.WriteInt(value));

        public static string ToASCIIString(this bool isLittleEndian, Action<DataProcessor> action)
        {
            using (var ms = new MemoryStream())
            {
                var processor = new DataProcessor(ms) { IsLittleEndian = isLittleEndian };
                action(processor);

                return Encoding.ASCII.GetStringUntil0(ms.ToArray());
            }

        }

    }

}
