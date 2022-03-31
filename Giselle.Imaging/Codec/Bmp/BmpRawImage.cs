using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Bmp
{
    public class BmpRawImage
    {
        public short Reserved1 { get; set; }
        public short Reserved2 { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public BmpBitsPerPixel BitsPerPixel { get; set; }
        public BmpCompressionMethod CompressionMethod { get; set; }
        public int WidthPixelsPerMeter { get; set; }
        public int HeightPixelsPerMeter { get; set; }
        public Argb32[] ColorTable { get; set; } = new Argb32[0];
        public byte[] ScanData { get; set; } = new byte[0];

        public int Stride => ScanProcessor.GetStride(this.Width, (short)this.BitsPerPixel);
    }

}
