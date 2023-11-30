using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public abstract class ColorTransformer
    {
        public ColorTransformer()
        {

        }

        public abstract Argb32 Encode(ScanData scanData, PointI coord, int tableIndex, Argb32 color);

        public abstract Argb32 Decode(ScanData scanData, PointI coord, int tableIndex, Argb32 color);

    }

}
