using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Utils
{
    public static class BitConverter2
    {
        public static FixLengthStringSerializer ASCIILittleEndian { get; } = new FixLengthStringSerializer() { IsLittleEndian = true };
        public static FixLengthStringSerializer ASCIIBigEndian { get; } = new FixLengthStringSerializer() { IsLittleEndian = false };

        public static (byte Upper, byte Lower) SplitNibbles(byte value)
        {
            var upper = (byte)((value >> 0x04) & 0x0F);
            var lower = (byte)((value >> 0x00) & 0x0F);
            return (upper, lower);
        }

    }

}
