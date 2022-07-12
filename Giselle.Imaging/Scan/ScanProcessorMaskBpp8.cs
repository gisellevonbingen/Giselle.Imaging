using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorMaskBpp8 : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor InstanceGrayscale8 { get; } = new ScanProcessorMaskBpp8()
        {
            RMask = 0xFF,
            GMask = 0xFF,
            BMask = 0xFF,
        };

        public ScanProcessorMaskBpp8()
        {

        }

        protected override void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset)
        {
            var merged = inputScan[inputOffset + 0];

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

            outputScan[outputOffset + 0] = (byte)(merged & 0xFF);
        }

    }

}
