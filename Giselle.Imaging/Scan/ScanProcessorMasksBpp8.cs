using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorMasksBpp8 : ScanProcessorInt32Masks
    {
        public ScanProcessorMasksBpp8()
        {

        }

        protected override void DecodePixel(byte[] inputScan, int inputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var merged = inputScan[inputOffset + 0];
            frame[coord] = this.DecodePixel(merged);
        }

        protected override void EncodePixel(byte[] outputScan, int outputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var color = frame[coord];
            var merged = this.EncodePixel(color);
            outputScan[outputOffset + 0] = (byte)(merged & 0xFF);
        }

    }

}
