using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Png
{
    public class PngImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte BitDepth { get; set; }
        public PngColorType ColorType { get; set; }
        public bool HasAlpha => (this.ColorType & PngColorType.WithAlphaMask) == PngColorType.WithAlphaMask;
        public byte Compression { get; set; }
        public byte Filter { get; set; }
        public byte Interlace { get; set; }
        public Argb32[] ColorTable { get; set; } = new Argb32[0];
        public MemoryStream CompressedScanData { get; set; } = new MemoryStream();

        public PixelFormat PixelFormat => PngColorTypeExtensions.ToPixelFormat(this.ColorType, this.BitDepth);

        public int Stride
        {
            get
            {
                var width = this.Width;
                var colorType = this.ColorType;
                var bitDepth = this.BitDepth;
                var bitsPerPixel = PngColorTypeExtensions.ToPixelFormat(colorType, bitDepth).GetBitsPerPixel();

                if (colorType == PngColorType.IndexedColor)
                {
                    var padding = (bitDepth % 8 == 0) ? 1 : 2;
                    return ScanProcessor.GetStride(width, bitsPerPixel, padding);
                }
                else if (colorType == PngColorType.Truecolor || colorType == PngColorType.TruecolorWithAlpha)
                {
                    return ScanProcessor.GetBytesPerWidth(width, bitsPerPixel);
                }
                else
                {
                    throw new NotImplementedException($"ColorType({colorType}) is Not Supported");
                }

            }

        }

    }

}