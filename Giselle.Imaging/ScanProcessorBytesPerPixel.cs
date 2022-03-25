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

        public override void Read(ScanData input, Image32Argb image)
        {
            var ibpp = input.Format.GetBitsPerPixel() / 8;
            var fbpp = this.FormatBitsPerPixel / 8;

            for (var y = 0; y < input.Height; y++)
            {
                var formatOffsetBase = y * image.Stride;
                var readingOffsetBase = y * input.Stride;

                for (var x = 0; x < input.Width; x++)
                {
                    var formatOffset = formatOffsetBase + (x * fbpp);
                    var inputOffset = readingOffsetBase + (x * ibpp);
                    this.ReadPixel(input.Scan, inputOffset, image.Scan, formatOffset);
                }

            }

        }

        public override void Write(ScanData output, Image32Argb image)
        {
            var obpp = output.Format.GetBitsPerPixel() / 8;
            var fbpp = this.FormatBitsPerPixel / 8;

            for (var y = 0; y < output.Height; y++)
            {
                var formatOffsetBase = y * image.Stride;
                var outputOffsetBase = y * output.Stride;

                for (var x = 0; x < output.Width; x++)
                {
                    var formatOffset = formatOffsetBase + (x * fbpp);
                    var outputOffset = outputOffsetBase + (x * obpp);
                    this.WritePixel(output.Scan, outputOffset, image.Scan, formatOffset);
                }

            }

        }

        protected abstract void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset);

        protected abstract void WritePixel(byte[] outputScan, int outputOffset, byte[] formatScan, int formatOffset);

    }

}
