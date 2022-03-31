using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorIndexed : ScanProcessor
    {
        public static ScanProcessor Instance { get; } = new ScanProcessorIndexed();

        public ScanProcessorIndexed()
        {

        }

        public override void Read(ScanData input, byte[] formatScan)
        {
            var mask = 0;
            var bpp = input.BitsPerPixel;
            var ppb = 8 / bpp;
            var dpp = this.FormatBitsPerPixel / 8;
            var formatStride = this.GetFormatStride(input.Width);

            for (var i = 0; i < bpp; i++)
            {
                mask |= 1 << i;
            }

            var index = 0;

            for (var y = 0; y < input.Height; y++)
            {
                var offsetBase = y * formatStride;

                for (var i = 0; i < input.Stride; i++)
                {
                    var b = input.Scan[index++];

                    for (var bi = 0; bi < ppb; bi++)
                    {
                        var x = i * ppb + bi;

                        if (x >= input.Width)
                        {
                            break;
                        }

                        var offset = offsetBase + (i * ppb * dpp) + (bi * dpp);

                        var shift = bpp * (ppb - 1 - bi);
                        var tableIndex = (b >> shift) & mask;
                        var color = input.ColorTable[tableIndex];
                        formatScan[offset + 0] = color.B;
                        formatScan[offset + 1] = color.G;
                        formatScan[offset + 2] = color.R;
                        formatScan[offset + 3] = color.A;
                    }

                }

            }

        }

        public override void Write(ScanData output, byte[] formatScan)
        {
            var maskBase = 0;
            var bpp = output.BitsPerPixel;
            var ppb = 8 / bpp;
            var formatStride = this.GetFormatStride(output.Width);

            for (var i = 0; i < bpp; i++)
            {
                maskBase |= 1 << i;
            }

            for (var y = 0; y < output.Height; y++)
            {
                for (var x = 0; x < output.Width; x++)
                {
                    var color = this.GetFormatColor(formatScan, formatStride, x, y);
                    var tableIndex = Array.IndexOf(output.ColorTable, color);

                    if (tableIndex == -1)
                    {
                        throw new IndexOutOfRangeException($"x:{x}, y:{y}'s color #{color} is not contains in ColorTable");
                    }

                    var index = (output.Stride * y) + (x / ppb);
                    var bi = x % ppb;
                    var shift = bpp * (ppb - 1 - bi);

                    var value = output.Scan[index];
                    value |= (byte)((tableIndex & maskBase) << shift);
                    output.Scan[index] = value;
                }

            }

        }

    }

}
