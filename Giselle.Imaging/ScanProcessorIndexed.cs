using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Bmp;

namespace Giselle.Imaging
{
    public class ScanProcessorIndexed : ScanProcessor
    {
        public static ScanProcessor Instance { get; } = new ScanProcessorIndexed();

        public ScanProcessorIndexed()
        {

        }

        public override void Read(ScanData reading, byte[] formatScan, int formatStride)
        {
            var maskBase = 0;
            var bpp = reading.Format.GetBitsPerPixel();
            var ppb = 8 / bpp;
            var dpp = this.FormatBitsPerPixel / 8;
            var width = reading.Width;
            var height = reading.Height;

            for (var i = 0; i < bpp; i++)
            {
                maskBase |= 1 << i;
            }

            var index = 0;

            for (var y = 0; y < height; y++)
            {
                var offsetBase = y * formatStride;

                for (var i = 0; i < reading.Stride; i++)
                {
                    var b = reading.Scan[index++];

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
                        var p = reading.ColorTable[tableIndex];
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
