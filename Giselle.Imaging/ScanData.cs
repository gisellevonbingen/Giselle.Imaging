using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public class ScanData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride { get; set; }
        public PixelFormat Format { get; set; }
        public byte[] Scan { get; set; }
        public Color[] ColorTable { get; set; }

        public ScanData(int width, int height, int stride, PixelFormat format)
            : this(width, height, stride, format, new byte[height * stride])
        {

        }

        public ScanData(int width, int height, int stride, PixelFormat format, byte[] scan)
            : this(width, height, stride, format, scan, null)
        {

        }

        public ScanData(int width, int height, int stride, PixelFormat format, byte[] scan, Color[] colorTable)
        {
            this.Width = width;
            this.Height = height;
            this.Stride = stride;
            this.Format = format;
            this.Scan = scan;
            this.ColorTable = colorTable ?? new Color[0];
        }

    }

}
