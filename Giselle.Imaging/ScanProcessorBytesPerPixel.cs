using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public abstract class ScanProcessorBytesPerPixel : ScanProcessor
    {
        public ScanProcessorBytesPerPixel()
        {

        }

        public override void Read(ScanData reading, byte[] formatScan, int formatStride)
        {
            var rbpp = reading.Format.GetBitsPerPixel() / 8;
            var fbpp = this.FormatBitsPerPixel / 8;
            var width = reading.Width;
            var height = reading.Height;
            var readingScan = reading.Scan;
            var readingStride = reading.Stride;

            for (var y = 0; y < height; y++)
            {
                var formatOffsetBase = y * formatStride;
                var readingOffsetBase = y * readingStride;

                for (var x = 0; x < width; x++)
                {
                    var formatOffset = formatOffsetBase + (x * fbpp);
                    var readingOffset = readingOffsetBase + (x * rbpp);
                    this.ReadPixel(formatScan, formatOffset, readingScan, readingOffset);
                }

            }

        }

        protected abstract void ReadPixel(byte[] formatScan, int formatOffset, byte[] readingScan, int readingOffset);

    }

}
