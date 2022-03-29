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
        
        Format16bppRgb555 = 10,
        Format16bppRgb565 = 11,
        Format16bppArgb1555 = 12,

        Format24bppRgb888 = 20,

        Format32bppRgb888 = 30,
        Format32bppArgb8888 = 31,
    }

}
