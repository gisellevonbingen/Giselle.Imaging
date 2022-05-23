using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.ICC
{
    public enum ICCColorSpaceType : int
    {
        PCSXYZ = 0x58595A20,
        PCSLABb = 0x4C616220,
        CIELUV = 0x4C757620,
        YCbCr = 0x59436272,
        CIEYxy = 0x59787920,
        RGB = 0x52474220,
        Gray = 0x47524159,
        HSV = 0x48535620,
        HLS = 0x484C5320,
        CMYK = 0x434D594B,
        CMY = 0x434D5920,
        Color2 = 0x32434C52,
        Color3 = 0x33434C52,
        Color4 = 0x34434C52,
        Color5 = 0x35434C52,
        Color6 = 0x36434C52,
        Color7 = 0x37434C52,
        Color8 = 0x38434C52,
        Color9 = 0x39434C52,
        Color10 = 0x41434C52,
        Color11 = 0x42434C52,
        Color12 = 0x43434C52,
        Color13 = 0x44434C52,
        Color14 = 0x45434C52,
        Color15 = 0x46434C52,
    }

}
