using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public abstract class ScanProcessorBytesPerPixel : ScanProcessor
    {
        public ScanProcessorBytesPerPixel(int width, int height, byte[] readingScan, int readingBitsPerPixel) : base(width, height, readingScan, readingBitsPerPixel)
        {

        }

        public override void Read(byte[] formatScan, int formatStride)
        {
            var rbpp = this.ReadingBitsPerPixel / 8;
            var fbpp = this.FormatBitsPerPixel / 8;
            var width = this.Width;
            var height = this.Height;
            var readingScan = this.ReadingScan;
            var readingStride = this.ReadingStride;

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
