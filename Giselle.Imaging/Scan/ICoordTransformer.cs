using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public interface ICoordTransformer
    {
        public delegate PointI Transform(ScanData scanData, PointI coord);

        PointI Encode(ScanData scanData, PointI coord);
        PointI Decode(ScanData scanData, PointI coord);
    }

}
