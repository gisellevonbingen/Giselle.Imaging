using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class RawImage
    {
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public byte[] Scan { get; }
        public PixelFormat Format { get; }

        public RawImage(int width, int height, int stride, byte[] scan, PixelFormat format)
        {
            this.Width = width;
            this.Height = height;
            this.Stride = stride;
            this.Scan = scan;
            this.Format = format;
        }

        public int Offset(int x, int y) => y * this.Stride + x;

        public Color this[int x, int y]
        {
            get
            {
                var offset = this.Offset(x, y);
                var b = this.Scan[offset + 0];
                var g = this.Scan[offset + 1];
                var r = this.Scan[offset + 2];
                var a = this.Scan[offset + 3];
                return Color.FromArgb(a, r, g, b);
            }

            set
            {
                var offset = this.Offset(x, y);
                this.Scan[offset + 0] = value.B;
                this.Scan[offset + 1] = value.G;
                this.Scan[offset + 2] = value.R;
                this.Scan[offset + 3] = value.A;
            }

        }

    }

}
