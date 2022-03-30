using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessor32 : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor InstanceRgb888 { get; } = new ScanProcessor32()
        {
            RMask = 0x00FF0000,
            GMask = 0x0000FF00,
            BMask = 0x000000FF,
        };

        public static ScanProcessor InstanceArgb8888 { get; } = new ScanProcessor32()
        {
            AMask = 0xFF000000,
            RMask = 0x00FF0000,
            GMask = 0x0000FF00,
            BMask = 0x000000FF,
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
            var merged = (b3 << 0x18) | (b2 << 0x10) | (b1 << 0x08) | (b0 << 0x00);

            formatScan[formatOffset + 0] = this.BMask.SplitByte(merged);
            formatScan[formatOffset + 1] = this.GMask.SplitByte(merged);
            formatScan[formatOffset + 2] = this.RMask.SplitByte(merged);
            formatScan[formatOffset + 3] = this.AMask.SplitByte(merged, byte.MaxValue);
        }

        protected override void WritePixel(byte[] outputScan, int outputOffset, byte[] formatScan, int formatOffset)
        {
            var merged = 0;
            merged = this.BMask.MergeByte(merged, formatScan[formatOffset + 0]);
            merged = this.GMask.MergeByte(merged, formatScan[formatOffset + 1]);
            merged = this.RMask.MergeByte(merged, formatScan[formatOffset + 2]);
            merged = this.AMask.MergeByte(merged, formatScan[formatOffset + 3]);

            outputScan[outputOffset + 0] = (byte)((merged >> 0x00) & 0xFF);
            outputScan[outputOffset + 1] = (byte)((merged >> 0x08) & 0xFF);
            outputScan[outputOffset + 2] = (byte)((merged >> 0x10) & 0xFF);
            outputScan[outputOffset + 3] = (byte)((merged >> 0x18) & 0xFF);
        }

    }

}
