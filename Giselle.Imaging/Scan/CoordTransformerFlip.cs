using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public class CoordTransformerFlip : ICoordTransformer
    {
        public bool FlipX { get; }
        public bool FlipY { get; }

        public CoordTransformerFlip(bool flipX, bool flipY)
        {
            this.FlipX = flipX;
            this.FlipY = flipY;
        }

        public PointI Decode(ScanData scanData, PointI coord)
        {
            return this.Flip(scanData, coord);
        }

        public PointI Encode(ScanData scanData, PointI coord)
        {
            return this.Flip(scanData, coord);
        }

        public PointI Flip(ScanData scanData, PointI coord)
        {
            var x = coord.X;
            var y = coord.Y;

            if (this.FlipX == true)
            {
                x = scanData.Width - 1 - coord.X;
            }

            if (this.FlipY == true)
            {
                y = scanData.Height - 1 - coord.Y;
            }

            return new PointI(x, y);
        }

    }

}
