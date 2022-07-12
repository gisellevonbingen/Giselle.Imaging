using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Scan
{
    public abstract class ScanProcessorInt32Mask : ScanProcessorBytesPerPixel
    {
        public Int32ChannelMask AMask { get; set; }
        public Int32ChannelMask RMask { get; set; }
        public Int32ChannelMask GMask { get; set; }
        public Int32ChannelMask BMask { get; set; }

        public ScanProcessorInt32Mask()
        {

        }

        protected Argb32 ReadPixel(int merged) => new Argb32()
        {
            B = this.BMask.SplitByte(merged),
            G = this.GMask.SplitByte(merged),
            R = this.RMask.SplitByte(merged),
            A = this.AMask.SplitByte(merged, byte.MaxValue),
        };

        protected int WritePixel(Argb32 color)
        {
            var merged = 0;
            merged = this.BMask.MergeByte(merged, color.B);
            merged = this.GMask.MergeByte(merged, color.G);
            merged = this.RMask.MergeByte(merged, color.R);
            merged = this.AMask.MergeByte(merged, color.A);
            return merged;
        }

    }

}
