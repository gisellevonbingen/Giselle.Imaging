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
        public ScanProcessorBytesPerPixel()
        {

        }

        public override void Read(ScanData input, ImageArgb32Frame frame)
        {
            this.ProcessFormat(input, frame, this.ReadPixel, input.GetDecodeCoord);
        }

        public override void Write(ScanData output, ImageArgb32Frame frame)
        {
            this.ProcessFormat(output, frame, this.WritePixel, output.GetEncodeCoord);
        }

        private void ProcessFormat(ScanData scan, ImageArgb32Frame frame, Action<byte[], int, ImageArgb32Frame, PointI> scanAction, Func<PointI, PointI> coordFunction)
        {
            var sbpp = scan.BitsPerPixel / 8;
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

                        scanAction(scan.Scan, scanOffset, frame, coord);
                        scanOffset += sbpp;
                    }

                    scanOffset += passInfo.Stride - (passInfo.PixelsX * sbpp);
                }

            }

        }

        protected abstract void ReadPixel(byte[] inputScan, int inputOffset, ImageArgb32Frame frame, PointI coord);

        protected abstract void WritePixel(byte[] outputScan, int outputOffset, ImageArgb32Frame frame, PointI coord);

    }

}
