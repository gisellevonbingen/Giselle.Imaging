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

        protected override Argb32 DecodePixel(byte[] inputScan, int inputOffset)
        {
            var merged = inputScan[inputOffset + 0];
            return this.DecodePixel(merged);
        }

        protected override void EncodePixel(byte[] outputScan, int outputOffset, Argb32 color)
        {
            var merged = this.EncodePixel(color);
            outputScan[outputOffset + 0] = (byte)(merged & 0xFF);
        }

    }

}
