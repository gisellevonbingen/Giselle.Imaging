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
                return ScanProcessor16.InstanceRgb555;
            }
            else if (format == PixelFormat.Format16bppRgb565)
            {
                return ScanProcessor16.InstanceRgb565;
            }
            else if (format == PixelFormat.Format16bppArgb1555)
            {
                return ScanProcessor16.InstanceArgb1555;
            }
            else if (format == PixelFormat.Format24bppRgb)
            {
                return ScanProcessor24.InstanceRgb888;
            }
            else if (format == PixelFormat.Format32bppRgb)
            {
                return ScanProcessor32.InstanceRgb888;
            }
            else if (format == PixelFormat.Format32bppArgb)
            {
                return ScanProcessor32.InstanceArgb8888;
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

        public abstract void Read(ScanData input, Image32Argb image);

        public abstract void Write(ScanData output, Image32Argb image);

        public int GetFormatStride(int width) => GetStride(width, this.FormatBitsPerPixel);

        public int FormatBitsPerPixel => this.FormatPixelFormat.GetBitsPerPixel();

        public PixelFormat FormatPixelFormat => PixelFormat.Format32bppArgb;

    }

}
