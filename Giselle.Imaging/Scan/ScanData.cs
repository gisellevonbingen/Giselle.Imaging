using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public class ScanData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride { get; set; }
        public int BitsPerPixel { get; set; }
        public byte[] Scan { get; set; }

        public Argb32[] ColorTable { get; set; } = new Argb32[0];

        public int InterlaceBlockWidth { get; set; } = 1;
        public int InterlaceBlockHeight { get; set; } = 1;
        public InterlacePass[] InterlacePasses { get; set; } = new InterlacePass[0];

        public CoordTransformer CoordTransformer { get; set; } = null;

        public ScanData(int width, int height, int bitsPerPixel)
        {
            this.Width = width;
            this.Height = height;
            this.BitsPerPixel = bitsPerPixel;
        }

        public int PreferredScanSize
        {
            get
            {
                var passes = this.InterlacePasses;

                if (passes.Length == 0)
                {
                    return this.Height * this.Stride;
                }
                else
                {
                    var totalBytes = 0;

                    for (var i = 0; i < passes.Length; i++)
                    {
                        var info = this.GetInterlacePassInformation(i);
                        totalBytes += info.PixelsY * info.Stride;
                    }

                    return totalBytes;
                }

            }

        }

        public InterlacePassInformation GetInterlacePassInformation(int interlacePassIndex)
        {
            var pass = this.InterlacePasses[interlacePassIndex];
            var pixelsX = ScanProcessor.GetPaddedQuotient(this.Width - pass.OffsetX, pass.IntervalX);
            var pixelsY = ScanProcessor.GetPaddedQuotient(this.Height - pass.OffsetY, pass.IntervalY);
            var stride = ScanProcessor.GetPaddedQuotient(pixelsX * this.BitsPerPixel, 8);
            return new InterlacePassInformation() { PixelsX = pixelsX, PixelsY = pixelsY, Stride = stride };
        }

        public PointI GetEncodeCoord(PointI coord) => this.CoordTransformer?.Encode(this, coord) ?? coord;

        public PointI GetDecodeCoord(PointI coord) => this.CoordTransformer?.Decode(this, coord) ?? coord;

    }

}
