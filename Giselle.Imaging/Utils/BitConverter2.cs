using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Utils
{
    public static class BitConverter2
    {
        public static (byte Upper, byte Lower) SplitNibbles(byte value)
        {
            var upper = (byte)((value >> 0x04) & 0x0F);
            var lower = (byte)((value >> 0x00) & 0x0F);
            return (upper, lower);
        }

    }

}
