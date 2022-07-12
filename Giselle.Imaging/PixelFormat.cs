using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public enum PixelFormat : byte
    {
        Undefined = 0,

        Format1bppIndexed = 1,
        Format2bppIndexed = 2,
        Format4bppIndexed = 3,
        Format8bppIndexed = 4,

        Format8bppGrayscale = 10,
        Format16bppAGrayscale = 11,

        Format16bppRgb555 = 20,
        Format16bppRgb565 = 21,
        Format16bppArgb1555 = 22,

        Format24bppRgb888 = 30,

        Format32bppRgb888 = 40,
        Format32bppArgb8888 = 41,

    }

}
