using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public interface IColorTransformer
    {
        public delegate Argb32 Transform(ScanData scanData, PointI coord, int tableIndex, Argb32 color);

        Argb32 Encode(ScanData scanData, PointI coord, int tableIndex, Argb32 color);
        Argb32 Decode(ScanData scanData, PointI coord, int tableIndex, Argb32 color);
    }

}
