using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ScanProcessor24 : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor InstanceRgb888 { get; } = new ScanProcessor24()
        {
            RMaskBits = 0x0000FF,
            GMaskBits = 0x00FF00,
            BMaskBits = 0xFF0000,
        };

        public ScanProcessor24()
        {

        }

        protected override void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset)
        {
            var b0 = inputScan[inputOffset + 0];
            var b1 = inputScan[inputOffset + 1];
            var b2 = inputScan[inputOffset + 2];
            var merged = b0 << 0x10 | b1 << 0x08 | b2 << 0x00;

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

            outputScan[outputOffset + 0] = (byte)((merged >> 0x10) & 0xFF);
            outputScan[outputOffset + 1] = (byte)((merged >> 0x08) & 0xFF);
            outputScan[outputOffset + 2] = (byte)((merged >> 0x00) & 0xFF);
        }

    }

}
