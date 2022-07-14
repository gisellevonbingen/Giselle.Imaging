using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorMasksBpp16 : ScanProcessorInt32Masks
    {
        public static ScanProcessor InstanceRgb555 { get; } = new ScanProcessorMasksBpp16()
        {
            RMask = 0x7C00,
            GMask = 0x03E0,
            BMask = 0x001F,
        };
        public static ScanProcessor InstanceRgb565 { get; } = new ScanProcessorMasksBpp16()
        {
            RMask = 0xF800,
            GMask = 0x07E0,
            BMask = 0x001F,
        };
        public static ScanProcessor InstanceArgb1555 { get; } = new ScanProcessorMasksBpp16()
        {
            AMask = 0x8000,
            RMask = 0x7C00,
            GMask = 0x03E0,
            BMask = 0x001F,
        };

        public ScanProcessorMasksBpp16()
        {

        }

        protected override void DecodePixel(byte[] inputScan, int inputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var b0 = inputScan[inputOffset + 0];
            var b1 = inputScan[inputOffset + 1];
            var merged = (b1 << 0x08) | (b0 << 0x00);
            frame[coord] = this.DecodePixel(merged);
        }

        protected override void EncodePixel(byte[] outputScan, int outputOffset, ImageArgb32Frame frame, PointI coord)
        {
            var color = frame[coord];
            var merged = this.EncodePixel(color);
            outputScan[outputOffset + 0] = (byte)((merged >> 0x00) & 0xFF);
            outputScan[outputOffset + 1] = (byte)((merged >> 0x08) & 0xFF);
        }

    }

}
