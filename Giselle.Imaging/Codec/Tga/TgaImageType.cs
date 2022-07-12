﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tga
{
    public enum TgaImageType : byte
    {
        NoImage = 0,
        ColorMapped = 1,
        TrueColor = 2,
        Grayscale = 3,
    }

}
