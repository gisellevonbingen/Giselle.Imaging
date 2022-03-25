using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ScanProcessor16 : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor InstanceRgb555 { get; } = new ScanProcessor16()
        {
            RMaskBits = 0x7C00,
            GMaskBits = 0x03E0,
            BMaskBits = 0x001F,
        };
        public static ScanProcessor InstanceRgb565 { get; } = new ScanProcessor16()
        {
            RMaskBits = 0xF800,
            GMaskBits = 0x07E0,
            BMaskBits = 0x001F,
        };
        public static ScanProcessor InstanceArgb1555 { get; } = new ScanProcessor16()
        {
            UseAlpha = true,
            AMaskBits = 0x8000,
            RMaskBits = 0x7C00,
            GMaskBits = 0x03E0,
            BMaskBits = 0x001F,
        };

        public ScanProcessor16()
        {

        }

        protected override void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset)
        {
            var b0 = inputScan[inputOffset + 0];
            var b1 = inputScan[inputOffset + 1];
            var merged = b0 << 0x08 | b1 << 0x00;

            formatScan[formatOffset + 0] = this.BMaskBits.SplitByte(merged);
            formatScan[formatOffset + 1] = this.GMaskBits.SplitByte(merged);
            formatScan[formatOffset + 2] = this.RMaskBits.SplitByte(merged);
            formatScan[formatOffset + 3] = this.UseAlpha ? this.AMaskBits.SplitByte(merged) : byte.MaxValue;
        }

        protected override void WritePixel(byte[] outputScan, int outputOffset, byte[] formatScan, int formatOffset)
        {
            var merged = 0;
            merged = this.BMaskBits.MergeByte(merged, formatScan[formatOffset + 0]);
            merged = this.GMaskBits.MergeByte(merged, formatScan[formatOffset + 1]);
            merged = this.RMaskBits.MergeByte(merged, formatScan[formatOffset + 2]);
            merged = this.UseAlpha ? this.AMaskBits.MergeByte(merged, formatScan[formatOffset + 3]) : merged;

            outputScan[outputOffset + 0] = (byte)((merged >> 0x08) & 0xFF);
            outputScan[outputOffset + 1] = (byte)((merged >> 0x00) & 0xFF);
        }

    }

}
