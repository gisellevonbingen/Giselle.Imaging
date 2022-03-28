using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Tiff
{
    [Flags]
    public enum TiffNewSubfileTypeFlag
    {
        None = 0,
        ReducedResolution = 1,
        SinglePageOfMultiPage = 2,
        DefinesTransparencyMask = 4,
    }

}
