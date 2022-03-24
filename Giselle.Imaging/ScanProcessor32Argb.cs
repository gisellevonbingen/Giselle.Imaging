using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ScanProcessor32Argb : ScanProcessorBytesPerPixel
    {
        public const int BitsPerPixel = 32;

        public ScanProcessor32Argb(int width, int height, byte[] readingScan) : base(width, height, readingScan, BitsPerPixel)
        {

        }

        protected override void ReadPixel(byte[] formatScan, int formatOffset, byte[] readingScan, int readingOffset)
        {
            formatScan[formatOffset + 0] = readingScan[readingOffset + 0];
            formatScan[formatOffset + 1] = readingScan[readingOffset + 1];
            formatScan[formatOffset + 2] = readingScan[readingOffset + 2];
            formatScan[formatOffset + 3] = readingScan[readingOffset + 3];
        }

    }

}
