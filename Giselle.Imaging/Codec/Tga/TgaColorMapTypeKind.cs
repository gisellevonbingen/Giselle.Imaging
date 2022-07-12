using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tga
{
    public enum TgaColorMapTypeKind : byte
    {
        NoColorMap = 0,
        Present = 1,
        ReservedByTruevision = 2,
        AvailableForDeveloper = 3,
    }

}
