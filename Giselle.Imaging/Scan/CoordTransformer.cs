using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public abstract class CoordTransformer
    {
        public CoordTransformer()
        {

        }

        public abstract PointI Encode(ScanData scanData, PointI coord);

        public abstract PointI Decode(ScanData scanData, PointI coord);

    }

}
