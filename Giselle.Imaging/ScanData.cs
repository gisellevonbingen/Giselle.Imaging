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
        public double WidthResoulution { get; set; }
        public double HeightResoulution { get; set; }
        public double Resolution
        {
            get => Math.Max(this.WidthResoulution, this.HeightResoulution);
            set { this.WidthResoulution = value; this.HeightResoulution = value; }
        }
        public bool UseBitFields { get; set; }
        public int BitFieldsBits { get; set; }
        public int AMask { get; set; }
        public int RMask { get; set; }
        public int GMask { get; set; }
        public int BMask { get; set; }

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
            this.Resolution = 96.0D;
        }

        public Bitmap ToBitmap()
        {
            unsafe
            {
                var stride = this.Stride;
                var format = this.Format;
                var scan = this.Scan;

                if (this.UseBitFields == true)
                {
                    var processor = ScanProcessor.CreateScanProcessor(this.BitFieldsBits, this.AMask, this.RMask, this.GMask, this.BMask);
                    stride = processor.GetFormatStride(this.Width);
                    format = processor.FormatPixelFormat;
                    scan = processor.Read(this);
                }

                fixed (byte* scan0 = scan)
                {
                    var bitmap = new Bitmap(this.Width, this.Height, stride, format, (IntPtr)scan0);
                    var colorTable = this.ColorTable;
                    var palette = bitmap.Palette;
                    var len = Math.Min(colorTable.Length, palette.Entries.Length);

                    if (len > 0)
                    {
                        for (var i = 0; i < len; i++)
                        {
                            palette.Entries[i] = colorTable[i];
                        }

                        bitmap.Palette = palette;
                    }

                    return bitmap;
                }

            }

        }

    }

}
