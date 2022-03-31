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
        public static byte[] ReadBytesWhile0(this DataProcessor processor)
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

    }

}
