using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Scan
{
    public struct InterlacePass
    {
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int IntervalX { get; set; }
        public int IntervalY { get; set; }

        public InterlacePass(int offsetX, int offsetY, int intervalX, int intervalY) : this()
        {
            this.OffsetX = offsetX;
            this.OffsetY = offsetY;
            this.IntervalX = intervalX;
            this.IntervalY = intervalY;
        }

    }

}
