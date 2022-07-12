using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Scan
{
    public abstract class ScanProcessor
    {
        public static ScanProcessor CreateScanProcessor(int bits, uint aMask, uint rMask, uint gMask, uint bMask)
        {
            if (bits == 1 || bits == 2 || bits == 4 || bits == 8)
            {
                return new ScanProcessorIndexed();
            }
            else if (bits == 16)
            {
                return new ScanProcessorMaskBpp16() { AMask = aMask, RMask = rMask, GMask = gMask, BMask = bMask };
            }
            else if (bits == 24)
            {
                return new ScanProcessorMaskBpp24() { AMask = aMask, RMask = rMask, GMask = gMask, BMask = bMask };
            }
            else if (bits == 32)
            {
                return new ScanProcessorMaskBpp32() { AMask = aMask, RMask = rMask, GMask = gMask, BMask = bMask };
            }
            else
            {
                throw new ArgumentException($"Unknown bits : {bits}");
            }

        }

        public static ScanProcessor GetScanProcessor(PixelFormat format)
        {
            if (format.GetColorTableLength() > 0)
            {
                return ScanProcessorIndexed.Instance;
            }
            else if (format == PixelFormat.Format8bppGrayscale)
            {
                return ScanProcessorMaskBpp8.InstanceGrayscale8;
            }
            else if (format == PixelFormat.Format16bppAGrayscale)
            {
                return ScanProcessorMaskBpp16.InstanceAGrayscale88;
            }
            else if (format == PixelFormat.Format16bppRgb555)
            {
                return ScanProcessorMaskBpp16.InstanceRgb555;
            }
            else if (format == PixelFormat.Format16bppRgb565)
            {
                return ScanProcessorMaskBpp16.InstanceRgb565;
            }
            else if (format == PixelFormat.Format16bppArgb1555)
            {
                return ScanProcessorMaskBpp16.InstanceArgb1555;
            }
            else if (format == PixelFormat.Format24bppRgb888)
            {
                return ScanProcessorMaskBpp24.InstanceRgb888;
            }
            else if (format == PixelFormat.Format32bppRgb888)
            {
                return ScanProcessorMaskBpp32.InstanceRgb888;
            }
            else if (format == PixelFormat.Format32bppArgb8888)
            {
                return ScanProcessorMaskBpp32.InstanceArgb8888;
            }
            else
            {
                throw new ArgumentException($"Unknown PixelFormat : {format}");
            }

        }

        public static int GetBytesPerWidth(int width, int bitsPerPixel)
        {
            if (bitsPerPixel < 8)
            {
                var w = 8 / bitsPerPixel;
                var bytes = width / w;
                return (width % w) == 0 ? bytes : bytes + 1;
            }
            else
            {
                var w = bitsPerPixel / 8;
                return width * w;
            }

        }

        public static int GetStride(int width, int bitsPerPixel, int padding)
        {
            var bytePerWidth = GetBytesPerWidth(width, bitsPerPixel);
            return ApplyPadding(bytePerWidth, padding);
        }

        public static int GetStride4(int width, int bitsPerPixel) => GetStride(width, bitsPerPixel, 4);

        public static int ApplyPadding(int bytePerWidth, int padding)
        {
            var mod = bytePerWidth % padding;
            var stride = mod == 0 ? bytePerWidth : (bytePerWidth - mod + padding);
            return stride;
        }

        public static int GetPaddedQuotient(int value, int padding) => ApplyPadding(value, padding) / padding;

        public ScanProcessor()
        {

        }

        public byte[] Read(ScanData input)
        {
            var stride = this.GetFormatStride(input.Width);
            var scan = new byte[input.Height * stride];
            this.Read(input, scan);
            return scan;
        }

        public abstract void Read(ScanData input, byte[] formatScan);

        public abstract void Write(ScanData output, byte[] formatScan);

        public Argb32 GetFormatColor(byte[] formatScan, int formatStride, PointI coord)
        {
            var offset = formatStride * coord.Y + (coord.X * 4);
            var b = formatScan[offset + 0];
            var g = formatScan[offset + 1];
            var r = formatScan[offset + 2];
            var a = formatScan[offset + 3];
            return new Argb32(a, r, g, b);
        }

        public int GetFormatStride(int width) => GetStride4(width, this.FormatBitsPerPixel);

        public int FormatBitsPerPixel => this.FormatPixelFormat.GetBitsPerPixel();

        public PixelFormat FormatPixelFormat => PixelFormat.Format32bppArgb8888;

    }

}
