using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class Image32Argb
    {
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public PixelFormat Format => PixelFormat.Format32bppArgb;
        public byte[] Scan { get; }

        public Image32Argb(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Stride = ScanProcessor.GetStride(width, ScanProcessor32Argb.BitsPerPixel);
            this.Scan = new byte[height * this.Stride];
        }

        public Image32Argb(ScanProcessor processor)
            : this(processor.Width, processor.Height, processor.FormatStride, processor.Read())
        {

        }

        public Image32Argb(int width, int height, int stride, byte[] scan)
        {
            this.Width = width;
            this.Height = height;
            this.Stride = stride;
            this.Scan = scan;
        }

        public int GetOffset(int x, int y) => y * this.Stride + x;

        public Color this[int x, int y]
        {
            get
            {
                var offset = this.GetOffset(x, y);
                var b = this.Scan[offset + 0];
                var g = this.Scan[offset + 1];
                var r = this.Scan[offset + 2];
                var a = this.Scan[offset + 3];
                return Color.FromArgb(a, r, g, b);
            }

            set
            {
                var offset = this.GetOffset(x, y);
                this.Scan[offset + 0] = value.B;
                this.Scan[offset + 1] = value.G;
                this.Scan[offset + 2] = value.R;
                this.Scan[offset + 3] = value.A;
            }

        }

        public Bitmap ToBitmap()
        {
            unsafe
            {
                fixed (byte* scan0 = this.Scan)
                {
                    return new Bitmap(this.Width, this.Height, this.Stride, this.Format, (IntPtr)scan0);
                }

            }

        }

    }

}
