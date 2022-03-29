using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Checksum;
using Giselle.Imaging.IO;

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
        public Color[] ColorTable { get; set; } = new Color[0];

        public int BitsPerPixel => PngColorTypeExtensions.GetBitsPerPixel(this.ColorType, this.BitDepth);

    }

}