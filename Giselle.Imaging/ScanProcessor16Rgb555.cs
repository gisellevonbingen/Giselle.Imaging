using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ScanProcessor16Rgb555 : ScanProcessorBytesPerPixel
    {
        public const int BitsPerPixel = 16;

        public ScanProcessor16Rgb555(int width, int height, byte[] readingScan) : base(width, height, readingScan, BitsPerPixel)
        {

        }

        protected override void ReadPixel(byte[] formatScan, int formatOffset, byte[] readingScan, int readingOffset)
        {
            var rgb = readingScan[readingOffset];
            formatScan[formatOffset + 0] = this.GetValue(rgb, 0x00);
            formatScan[formatOffset + 1] = this.GetValue(rgb, 0x05);
            formatScan[formatOffset + 2] = this.GetValue(rgb, 0x0A);
            formatScan[formatOffset + 3] = 255;
        }

        protected byte GetValue(int rgb, int shift)
        {
            var maskBase = 0x1F;
            var mask = maskBase << shift;
            var raw = (byte)((rgb & mask) >> shift);
            return (byte)((raw * 255) / maskBase);
        }

    }

}
