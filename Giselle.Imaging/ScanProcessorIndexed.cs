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

        public override void Read(ScanData input, Image32Argb image)
        {
            var mask = 0;
            var bpp = input.Format.GetBitsPerPixel();
            var ppb = 8 / bpp;
            var dpp = this.FormatBitsPerPixel / 8;

            for (var i = 0; i < bpp; i++)
            {
                mask |= 1 << i;
            }

            var index = 0;

            for (var y = 0; y < input.Height; y++)
            {
                var offsetBase = y * image.Stride;

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
                        image.Scan[offset + 0] = color.B;
                        image.Scan[offset + 1] = color.G;
                        image.Scan[offset + 2] = color.R;
                        image.Scan[offset + 3] = color.A;
                    }

                }

            }

        }

        public override void Write(ScanData output, Image32Argb image)
        {
            var maskBase = 0;
            var bpp = output.Format.GetBitsPerPixel();
            var ppb = 8 / bpp;

            for (var i = 0; i < bpp; i++)
            {
                maskBase |= 1 << i;
            }

            for (var y = 0; y < output.Height; y++)
            {
                for (var x = 0; x < output.Width; x++)
                {
                    var color = image[x, y];
                    var tableIndex = Array.IndexOf(output.ColorTable, color);

                    if (tableIndex == -1)
                    {
                        throw new IndexOutOfRangeException($"x:{x}, y:{y}'s color ${color} is not contains in ColorTable");
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
