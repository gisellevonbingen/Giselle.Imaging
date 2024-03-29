﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formats.Riff;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public abstract class ScanProcessorBytesPerPixel : ScanProcessor
    {
        public ScanProcessorBytesPerPixel()
        {

        }

        public override void Decode(ScanData input, ImageArgb32Frame frame)
        {
            this.Process(input, frame, this.DecodePixel, input.GetDecodeCoord);
        }

        public override void Encode(ScanData output, ImageArgb32Frame frame)
        {
            this.Process(output, frame, this.EncodePixel, output.GetEncodeCoord);
        }

        private void DecodePixel(ScanData input, int inputOffset, ImageArgb32Frame frame, PointI coord)
        {
            frame[coord] = input.GetDecodeColor(coord, -1, this.DecodePixel(input.Scan, inputOffset));
        }

        private void EncodePixel(ScanData output, int outputOffset, ImageArgb32Frame frame, PointI coord)
        {
            this.EncodePixel(output.Scan, outputOffset, output.GetEncodeColor(coord, -1, frame[coord]));
        }

        private void Process(ScanData scan, ImageArgb32Frame frame, Action<ScanData, int, ImageArgb32Frame, PointI> scanAction, Func<PointI, PointI> coordTransoform)
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
                        var coord = coordTransoform(passProcessor.GetPixelCoord(new PointI(xi, yi)));

                        scanAction(scan, scanOffset, frame, coord);
                        scanOffset += sbpp;
                    }

                    scanOffset += passInfo.Stride - (passInfo.PixelsX * sbpp);
                }

            }

        }

        protected abstract Argb32 DecodePixel(byte[] inputScan, int inputOffset);

        protected abstract void EncodePixel(byte[] outputScan, int outputOffset, Argb32 color);
    }

}
