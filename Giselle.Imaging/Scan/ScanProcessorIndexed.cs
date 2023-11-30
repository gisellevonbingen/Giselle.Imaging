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
        public static ScanProcessor BlackIsZero { get; } = new ScanProcessorIndexed() { PresetColorTable = new[] { Argb32.Black, Argb32.White } };
        public static ScanProcessor WhiteIsZero { get; } = new ScanProcessorIndexed() { PresetColorTable = new[] { Argb32.White, Argb32.Black } };

        public Argb32[] PresetColorTable { get; set; }

        public ScanProcessorIndexed()
        {

        }

        public Argb32[] GetUsingColorTable(ScanData scan) => this.GetUsingColorTable(scan.ColorTable);

        public Argb32[] GetUsingColorTable(Argb32[] original)
        {
            return this.PresetColorTable ?? original;
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

            var colorTable = this.GetUsingColorTable(input);
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
                            var orignalX = xi * ppb + bi;

                            if (orignalX >= input.Width)
                            {
                                break;
                            }

                            var coord = passProcessor.GetDecodeCoord(new PointI(xi * ppb + bi, yi));
                            var shift = bpp * (ppb - 1 - bi);
                            var tableIndex = (b >> shift) & mask;
                            var color = colorTable[tableIndex];
                            frame[coord] = input.GetDecodeColor(coord, tableIndex, color);
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

            var colorTable = this.GetUsingColorTable(output);
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

                            var original = frame[coord];
                            var originalTableIndex = Array.IndexOf(colorTable, original);
                            var color = output.GetEncodeColor(coord, originalTableIndex, original);
                            var tableIndex = original == color ? originalTableIndex : Array.IndexOf(colorTable, color);

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
