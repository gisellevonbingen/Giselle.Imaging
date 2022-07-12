using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorMaskBpp16 : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor InstanceAGrayscale88 { get; } = new ScanProcessorMaskBpp16()
        {
            AMask = 0xFF00,
            RMask = 0x00FF,
            GMask = 0x00FF,
            BMask = 0x00FF,
        };
        public static ScanProcessor InstanceRgb555 { get; } = new ScanProcessorMaskBpp16()
        {
            RMask = 0x7C00,
            GMask = 0x03E0,
            BMask = 0x001F,
        };
        public static ScanProcessor InstanceRgb565 { get; } = new ScanProcessorMaskBpp16()
        {
            RMask = 0xF800,
            GMask = 0x07E0,
            BMask = 0x001F,
        };
        public static ScanProcessor InstanceArgb1555 { get; } = new ScanProcessorMaskBpp16()
        {
            AMask = 0x8000,
            RMask = 0x7C00,
            GMask = 0x03E0,
            BMask = 0x001F,
        };

        public ScanProcessorMaskBpp16()
        {

        }

        protected override void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset)
        {
            var b0 = inputScan[inputOffset + 0];
            var b1 = inputScan[inputOffset + 1];
            var merged = (b1 << 0x08) | (b0 << 0x00);

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
        }

    }

}
