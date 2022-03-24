using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Bmp
{
    public enum BmpBitsPerPixel : byte
    {
        Indexed1 = 1,
        Indexed4 = 4,
        Indexed8 = 8,
        Rgb24 = 24,
        Argb32 = 32,
    }

}
