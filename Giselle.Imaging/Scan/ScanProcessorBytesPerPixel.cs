using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Scan
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

            var passProcessor = new InterlacePassProcessor(input);
            var index = 0;

            while (passProcessor.NextPass() == true)
            {
                var passInfo = passProcessor.PassInfo;

                for (var yi = 0; yi < passInfo.PixelsY; yi++)
                {
                    for (var xi = 0; xi < passInfo.PixelsX; xi++)
                    {
                        (var x, var y) = passProcessor.GetPosition(xi, yi);

                        if (y >= input.Height)
                        {
                            break;
                        }

                        var formatOffset = (y * formatStirde) + (x * fbpp);
                        this.ReadPixel(input.Scan, index, formatScan, formatOffset);
                        index += ibpp;
                    }

                    index += passInfo.Stride - (passInfo.PixelsX * ibpp);
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
