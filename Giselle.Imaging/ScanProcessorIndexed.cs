using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ScanProcessorIndexed : ScanProcessor
    {
        public Color[] ColorTable { get; set; }

        public ScanProcessorIndexed(int width, int height, byte[] readingScan, int bitsPerPixel)
            : base(width, height, readingScan, bitsPerPixel)
        {
            this.ColorTable = new Color[0];
        }

        public override void Read(byte[] formatScan, int formatStride)
        {
            var maskBase = 0;
            var bpp = this.ReadingBitsPerPixel;
            var ppb = 8 / bpp;
            var dpp = this.FormatBitsPerPixel / 8;
            var width = this.Width;
            var height = this.Height;

            for (var i = 0; i < bpp; i++)
            {
                maskBase |= 1 << i;
            }

            var index = 0;

            for (var y = 0; y < height; y++)
            {
                var offsetBase = y * formatStride;

                for (var i = 0; i < this.ReadingStride; i++)
                {
                    var b = this.ReadingScan[index++];

                    for (var bi = 0; bi < ppb; bi++)
                    {
                        var x = i * ppb + bi;

                        if (x >= width)
                        {
                            break;
                        }

                        var offset = offsetBase + (i * ppb * dpp) + (bi * dpp);

                        var shift = bpp * (ppb - 1 - bi);
                        var mask = maskBase << shift;
                        var tableIndex = (b & mask) >> shift;
                        var p = this.ColorTable[tableIndex];
                        formatScan[offset + 0] = p.B;
                        formatScan[offset + 1] = p.G;
                        formatScan[offset + 2] = p.R;
                        formatScan[offset + 3] = p.A;
                    }

                }

            }

        }

    }

}
