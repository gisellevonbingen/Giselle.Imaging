using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorMasksBpp32 : ScanProcessorInt32Masks
    {
        public static ScanProcessor InstanceRgb888 { get; } = new ScanProcessorMasksBpp32()
        {
            RMask = 0x00FF0000,
            GMask = 0x0000FF00,
            BMask = 0x000000FF,
        };

        public static ScanProcessor InstanceArgb8888 { get; } = new ScanProcessorMasksBpp32()
        {
            AMask = 0xFF000000,
            RMask = 0x00FF0000,
            GMask = 0x0000FF00,
            BMask = 0x000000FF,
        };

        public ScanProcessorMasksBpp32()
        {

        }

        protected override Argb32 DecodePixel(byte[] inputScan, int inputOffset)
        {
            var b0 = inputScan[inputOffset + 0];
            var b1 = inputScan[inputOffset + 1];
            var b2 = inputScan[inputOffset + 2];
            var b3 = inputScan[inputOffset + 3];
            var merged = (b3 << 0x18) | (b2 << 0x10) | (b1 << 0x08) | (b0 << 0x00);
            return this.DecodePixel(merged);
        }

        protected override void EncodePixel(byte[] outputScan, int outputOffset, Argb32 color)
        {
            var merged = this.EncodePixel(color);
            outputScan[outputOffset + 0] = (byte)((merged >> 0x00) & 0xFF);
            outputScan[outputOffset + 1] = (byte)((merged >> 0x08) & 0xFF);
            outputScan[outputOffset + 2] = (byte)((merged >> 0x10) & 0xFF);
            outputScan[outputOffset + 3] = (byte)((merged >> 0x18) & 0xFF);
        }

    }

}
