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
        public static ScanProcessor GetScanProcessor(PixelFormat format)
        {
            if (format == PixelFormat.Format1bppIndexed || format == PixelFormat.Format4bppIndexed || format == PixelFormat.Format8bppIndexed)
            {
                return ScanProcessorIndexed.Instance;
            }
            else if (format == PixelFormat.Format16bppRgb555)
            {
                return ScanProcessor16Rgb555.Instance;
            }
            else if (format == PixelFormat.Format24bppRgb)
            {
                return ScanProcessor24Rgb.Instance;
            }
            else if (format == PixelFormat.Format32bppArgb)
            {
                return ScanProcessor32Argb.Instance;
            }
            else
            {
                throw new ArgumentException($"Unknown PixelFormat : {format}");
            }

        }

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

        public ScanProcessor()
        {

        }

        public byte[] Read(ScanData reading)
        {
            var formatStride = GetStride(reading.Width, this.FormatBitsPerPixel);
            var formatScan = new byte[reading.Height * formatStride];
            this.Read(reading, formatScan, formatStride);
            return formatScan;
        }

        public abstract void Read(ScanData reading, byte[] formatScan, int formatStride);

        public int GetFormatBitsPerPixel(int width) => GetStride(width, this.FormatBitsPerPixel);

        public int FormatBitsPerPixel => this.FormatPixelFormat.GetBitsPerPixel();

        public PixelFormat FormatPixelFormat => PixelFormat.Format32bppArgb;

    }

}
