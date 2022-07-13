using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public static class DataProcessorExtensions
    {
        public static byte[] ReadBytesUntil0(this DataProcessor processor)
        {
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    var b = processor.ReadByte();

                    if (b == 0x00)
                    {
                        return ms.ToArray();
                    }
                    else
                    {
                        ms.WriteByte(b);
                    }

                }

            }

        }

        public static void WriteBytesWith0(this DataProcessor processor, byte[] bytes)
        {
            using (var ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.WriteByte(0x00);

                processor.WriteBytes(ms.ToArray());
            }

        }

        public static void ReadArray<T>(this DataProcessor processor, T[] values, int offset, int count, Func<DataProcessor, T> func)
        {
            for (var i = 0; i < count; i++)
            {
                values[offset + i] = func(processor);
            }

        }

        public static T[] ReadArray<T>(this DataProcessor processor, int count, Func<DataProcessor, T> func)
        {
            var array = new T[count];
            ReadArray(processor, array, 0, count, func);
            return array;
        }

    }

}
