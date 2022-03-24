using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public abstract class ScanProcessor
    {
        public static int GetStride(int width, int bitsPerPixel)
        {
            var divisor = 4;
            var w1 = bitsPerPixel <= 8 ? 1 : (bitsPerPixel / 8);
            var w2 = bitsPerPixel < 8 ? (8 / bitsPerPixel) : 1;

            var readingWidth = (width * w1) / w2;
            var readingMod = readingWidth % divisor;
            var stride = readingMod == 0 ? readingWidth : (readingWidth - readingMod + divisor);
            return stride;
        }

        public int Width { get; }
        public int Height { get; }
        public byte[] ReadingScan { get; }
        public int ReadingStride { get; set; }
        public int ReadingBitsPerPixel { get; }

        public ScanProcessor(int width, int height, byte[] readingScan, int readingBitsPerPixel)
        {
            this.Width = width;
            this.Height = height;
            this.ReadingScan = readingScan;
            this.ReadingStride = GetStride(width, readingBitsPerPixel);
            this.ReadingBitsPerPixel = readingBitsPerPixel;
        }

        public byte[] Read() => this.Read(this.FormatStride);

        public byte[] Read(int formatStride)
        {
            var formatScan = new byte[this.Height * formatStride];
            this.Read(formatScan, formatStride);
            return formatScan;
        }

        public abstract void Read(byte[] formatScan, int formatStride);

        public int FormatStride => GetStride(this.Width, this.FormatBitsPerPixel);

        public int FormatBitsPerPixel => 32;

        public PixelFormat FormatPixelFormat => PixelFormat.Format32bppArgb;

    }

}
