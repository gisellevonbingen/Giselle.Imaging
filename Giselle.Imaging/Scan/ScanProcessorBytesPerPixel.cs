using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

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
            this.ProcessFormat(input, formatScan, this.ReadPixel, input.GetDecodeCoord);
        }

        public override void Write(ScanData output, byte[] formatScan)
        {
            this.ProcessFormat(output, formatScan, this.WritePixel, output.GetEncodeCoord);
        }

        private void ProcessFormat(ScanData scan, byte[] formatScan, Action<byte[], int, byte[], int> scanAction, Func<PointI, PointI> coordFunction)
        {
            var sbpp = scan.BitsPerPixel / 8;
            var fbpp = this.FormatBitsPerPixel / 8;
            var formatStride = this.GetFormatStride(scan.Width);

            var passProcessor = new InterlacePassProcessor(scan);
            var scanOffset = 0;

            while (passProcessor.NextPass() == true)
            {
                var passInfo = passProcessor.PassInfo;

                for (var yi = 0; yi < passInfo.PixelsY; yi++)
                {
                    for (var xi = 0; xi < passInfo.PixelsX; xi++)
                    {
                        var coord = coordFunction(new PointI(xi, yi));

                        var formatOffset = (coord.Y * formatStride) + (coord.X * fbpp);
                        scanAction(scan.Scan, scanOffset, formatScan, formatOffset);
                        scanOffset += sbpp;
                    }

                    scanOffset += passInfo.Stride - (passInfo.PixelsX * sbpp);
                }

            }

        }

        protected abstract void ReadPixel(byte[] inputScan, int inputOffset, byte[] formatScan, int formatOffset);

        protected abstract void WritePixel(byte[] outputScan, int outputOffset, byte[] formatScan, int formatOffset);

    }

}
