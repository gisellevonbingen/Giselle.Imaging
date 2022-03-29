using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public abstract class ScanProcessorBytesPerPixel : ScanProcessor
    {
        public Int32ChannelMask AMask { get; set; }
        public Int32ChannelMask RMask { get; set; }
        public Int32ChannelMask GMask { get; set; }
        public Int32ChannelMask BMask { get; set; }

        public ScanProcessorBytesPerPixel()
        {

        }

        public override void Read(ScanData input, byte[] formatScan)
        {
            var ibpp = input.BitsPerPixel / 8;
            var fbpp = this.FormatBitsPerPixel / 8;
            var formatStirde = this.GetFormatStride(input.Width);

            for (var y = 0; y < input.Height; y++)
            {
                var formatOffsetBase = y * formatStirde;
                var readingOffsetBase = y * input.Stride;

                for (var x = 0; x < input.Width; x++)
                {
                    var formatOffset = formatOffsetBase + (x * fbpp);
                    var inputOffset = readingOffsetBase + (x * ibpp);
                    this.ReadPixel(input.Scan, inputOffset, formatScan, formatOffset);
                }

            }

        }

        public override void Write(ScanData output, byte[] formatScan)
        {
            var obpp = output.BitsPerPixel / 8;
            var fbpp = this.FormatBitsPerPixel / 8;
            var formatStirde = this.GetFormatStride(output.Width);

            for (var y = 0; y < output.Height; y++)
            {
                var formatOffsetBase = y * formatStirde;
                var outputOffsetBase = y * output.Stride;

                for (var x = 0; x < output.Width; x++)
                {
                    var formatOffset = formatOffsetBase + (x * fbpp);
                    var outputOffset = outputOffsetBase + (x * obpp);
                    this.WritePixel(output.Scan, outputOffset, formatScan, formatOffset);
                }

            }

        }

        protected abstract void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset);

        protected abstract void WritePixel(byte[] outputScan, int outputOffset, byte[] formatScan, int formatOffset);

    }

}
