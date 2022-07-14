using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public class ScanProcessorIndexed : ScanProcessor
    {
        public static ScanProcessor Instance { get; } = new ScanProcessorIndexed();

        public ScanProcessorIndexed()
        {

        }

        public Argb32[] GetColorTableWitFallback(ScanData scan) => this.GetColorTableWitFallback(scan.ColorTable, scan.BitsPerPixel);

        public Argb32[] GetColorTableWitFallback(Argb32[] original, int bitsPerPixel)
        {
            if (original.Length == 0 && bitsPerPixel == 1)
            {
                return new Argb32[] { Argb32.Black, Argb32.White };
            }
            else
            {
                return original;
            }

        }

        public override void Decode(ScanData input, ImageArgb32Frame frame)
        {
            var mask = 0;
            var bpp = input.BitsPerPixel;
            var ppb = 8 / bpp;

            for (var i = 0; i < bpp; i++)
            {
                mask |= 1 << i;
            }

            var colorTable = this.GetColorTableWitFallback(input);
            var passProcessor = new InterlacePassProcessor(input);
            var index = 0;

            while (passProcessor.NextPass() == true)
            {
                var passInfo = passProcessor.PassInfo;

                for (var yi = 0; yi < passInfo.PixelsY; yi++)
                {
                    for (var xi = 0; xi < passInfo.Stride; xi++)
                    {
                        var b = input.Scan[index++];

                        for (var bi = 0; bi < ppb; bi++)
                        {
                            var coord = passProcessor.GetDecodeCoord(new PointI(xi * ppb + bi, yi));

                            if (coord.X >= input.Width)
                            {
                                break;
                            }

                            var shift = bpp * (ppb - 1 - bi);
                            var tableIndex = (b >> shift) & mask;
                            var color = colorTable[tableIndex];
                            frame[coord] = color;
                        }

                    }

                }

            }

        }

        public override void Encode(ScanData output, ImageArgb32Frame frame)
        {
            var maskBase = 0;
            var bpp = output.BitsPerPixel;
            var ppb = 8 / bpp;

            for (var i = 0; i < bpp; i++)
            {
                maskBase |= 1 << i;
            }

            var colorTable = this.GetColorTableWitFallback(output);
            var passProcessor = new InterlacePassProcessor(output);
            var index = 0;

            while (passProcessor.NextPass() == true)
            {
                var passInfo = passProcessor.PassInfo;

                for (var yi = 0; yi < passInfo.PixelsY; yi++)
                {
                    for (var xi = 0; xi < passInfo.Stride; xi++)
                    {
                        var value = output.Scan[index];

                        for (var bi = 0; bi < ppb; bi++)
                        {
                            var coord = passProcessor.GetEncodeCoord(new PointI(xi * ppb + bi, yi));

                            if (coord.X >= output.Width)
                            {
                                break;
                            }

                            var color = frame[coord];
                            var tableIndex = Array.IndexOf(colorTable, color);

                            if (tableIndex == -1)
                            {
                                throw new IndexOutOfRangeException($"Coord:{coord}'s color #{color} is not contains in ColorTable");
                            }

                            var shift = bpp * (ppb - 1 - bi);
                            value |= (byte)((tableIndex & maskBase) << shift);
                        }

                        output.Scan[index++] = value;
                    }

                }

            }

        }

    }

}
