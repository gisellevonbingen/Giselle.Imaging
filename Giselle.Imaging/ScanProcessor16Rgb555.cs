using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ScanProcessor16Rgb555 : ScanProcessorBytesPerPixel
    {
        public static ScanProcessor Instance { get; } = new ScanProcessor16Rgb555();

        public ScanProcessor16Rgb555()
        {

        }

        protected override void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset)
        {
            var rgb = inputScan[inputOffset];
            formatScan[formatOffset + 0] = this.SplitValue(rgb, 0x00);
            formatScan[formatOffset + 1] = this.SplitValue(rgb, 0x05);
            formatScan[formatOffset + 2] = this.SplitValue(rgb, 0x0A);
            formatScan[formatOffset + 3] = 255;
        }

        protected override void WritePixel(byte[] outputScan, int outputOffset, byte[] formatScan, int formatOffset)
        {
            byte rgb = 0;
            rgb = this.MergeValue(rgb, 0x00, formatScan[formatOffset + 0]);
            rgb = this.MergeValue(rgb, 0x05, formatScan[formatOffset + 1]);
            rgb = this.MergeValue(rgb, 0x0A, formatScan[formatOffset + 2]);
            outputScan[outputOffset] = rgb;
        }

        protected byte SplitValue(byte rgb, int shift)
        {
            var mask = 0x1F;
            var raw = (rgb >> shift) & mask;
            return (byte)((raw * 255) / mask);
        }

        protected byte MergeValue(int rgb, int shift, byte value)
        {
            var mask = 0x1F;
            var raw = (value * mask) / 255;
            return (byte)(rgb | ((raw & 0x1F) << shift));
        }

    }

}
