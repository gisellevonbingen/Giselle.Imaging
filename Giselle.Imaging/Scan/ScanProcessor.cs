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
        public static ScanProcessor CreateScanProcessor(int bitsPerPixel, uint aMask, uint rMask, uint gMask, uint bMask)
        {
            if (bitsPerPixel == 1 || bitsPerPixel == 2 || bitsPerPixel == 4 || bitsPerPixel == 8)
            {
                return new ScanProcessorIndexed();
            }
            else if (bitsPerPixel == 16)
            {
                return new ScanProcessorMasksBpp16() { AMask = aMask, RMask = rMask, GMask = gMask, BMask = bMask };
            }
            else if (bitsPerPixel == 24)
            {
                return new ScanProcessorMasksBpp24() { AMask = aMask, RMask = rMask, GMask = gMask, BMask = bMask };
            }
            else if (bitsPerPixel == 32)
            {
                return new ScanProcessorMasksBpp32() { AMask = aMask, RMask = rMask, GMask = gMask, BMask = bMask };
            }
            else
            {
                throw new ArgumentException($"Unknown bits : {bitsPerPixel}");
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
                return ScanProcessorGrayscaleBpp8.InstanceGrayscale8;
            }
            else if (format == PixelFormat.Format16bppAGrayscale)
            {
                return ScanProcessorGrayscaleBpp16.InstanceAGrayscale88;
            }
            else if (format == PixelFormat.Format16bppRgb555)
            {
                return ScanProcessorMasksBpp16.InstanceRgb555;
            }
            else if (format == PixelFormat.Format16bppRgb565)
            {
                return ScanProcessorMasksBpp16.InstanceRgb565;
            }
            else if (format == PixelFormat.Format16bppArgb1555)
            {
                return ScanProcessorMasksBpp16.InstanceArgb1555;
            }
            else if (format == PixelFormat.Format24bppRgb888)
            {
                return ScanProcessorMasksBpp24.InstanceRgb888;
            }
            else if (format == PixelFormat.Format32bppRgb888)
            {
                return ScanProcessorMasksBpp32.InstanceRgb888;
            }
            else if (format == PixelFormat.Format32bppArgb8888)
            {
                return ScanProcessorMasksBpp32.InstanceArgb8888;
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

        public abstract void Decode(ScanData input, ImageArgb32Frame frame);

        public abstract void Encode(ScanData output, ImageArgb32Frame frame);

    }

}
