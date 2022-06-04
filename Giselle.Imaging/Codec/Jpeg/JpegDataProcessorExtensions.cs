using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;
using Giselle.Imaging.Utils;

namespace Giselle.Imaging.Codec.Jpeg
{
    public static class JpegStreamExtensions
    {
        public static int ReadDC(this JpegEntropyStream stream)
        {
            var length = stream.ReadByte();
            return ReadDC(stream, length);
        }

        public static int ReadDC(this JpegEntropyStream stream, int length)
        {
            var value = 0;

            for (var i = 0; i < length; i++)
            {
                value = (value << 1) + stream.ReadBit();
            }

            var range = 1 << (length - 1);

            if (value < range)
            {
                return value - (range << 1) + 1;
            }
            else
            {
                return value;
            }

        }

        public static void ReadACTable(this JpegEntropyStream stream, int[] table, int offset, int count)
        {
            for (var i = 0; i < count;)
            {
                var rs = stream.ReadByte();
                var (r, s) = BitConverter2.SplitNibbles((byte)rs);

                if (s == 0)
                {
                    if (r == 15)
                    {
                        i += 16;
                    }
                    else
                    {
                        break;
                    }

                }
                else
                {
                    i += r;
                    var diff = stream.ReadDC(s);
                    table[offset + i] = diff;
                    i++;
                }

            }

        }

    }

}
