using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ScanProcessor32 : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor InstanceRgb888 { get; } = new ScanProcessor32()
        {
            RMaskBits = 0x000000FF,
            GMaskBits = 0x0000FF00,
            BMaskBits = 0x00FF0000,
        };

        public static ScanProcessor InstanceArgb8888 { get; } = new ScanProcessor32()
        {
            UseAlpha = true,
            AMaskBits = 0xFF000000,
            RMaskBits = 0x000000FF,
            GMaskBits = 0x0000FF00,
            BMaskBits = 0x00FF0000,
        };

        public ScanProcessor32()
        {

        }

        protected override void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset)
        {
            var b0 = inputScan[inputOffset + 0];
            var b1 = inputScan[inputOffset + 1];
            var b2 = inputScan[inputOffset + 2];
            var b3 = inputScan[inputOffset + 3];
            var merged = b0 << 0x18 | b1 << 0x10 | b2 << 0x08 | b3 << 0x00;

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

            outputScan[outputOffset + 0] = (byte)((merged >> 0x18) & 0xFF);
            outputScan[outputOffset + 1] = (byte)((merged >> 0x10) & 0xFF);
            outputScan[outputOffset + 2] = (byte)((merged >> 0x08) & 0xFF);
            outputScan[outputOffset + 3] = (byte)((merged >> 0x00) & 0xFF);
        }

    }

}
