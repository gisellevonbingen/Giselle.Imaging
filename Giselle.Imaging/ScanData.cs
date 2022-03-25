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
        public int Width { get; }
        public int Height { get; }
        public int Stride { get; }
        public PixelFormat Format { get; }
        public byte[] Scan { get; }
        public Color[] ColorTable { get; }
        public double WidthResoulution { get; set; }
        public double HeightResoulution { get; set; }
        public double Resolution
        {
            get => Math.Max(this.WidthResoulution, this.HeightResoulution);
            set { this.WidthResoulution = value; this.HeightResoulution = value; }
        }

        public ScanData(Image32Argb image)
        {
            this.Width = image.Width;
            this.Height = image.Height;
            this.Stride = image.Stride;
            this.Scan = image.Scan;
            this.ColorTable = new Color[0];
            this.Resolution = image.Resolution;
        }

        public ScanData(int width, int height, int stride, PixelFormat format, byte[] scan, Color[] colorTable)
        {
            this.Width = width;
            this.Height = height;
            this.Stride = stride;
            this.Format = format;
            this.Scan = scan;
            this.ColorTable = colorTable;
            this.Resolution = 96.0D;
        }

        public ScanProcessor CreateProcessor()
        {
            var format = this.Format;
            var width = this.Width;
            var height = this.Height;
            var scan = this.Scan;
            var colorTable = this.ColorTable;

            if (format == PixelFormat.Format1bppIndexed)
            {
                return new ScanProcessorIndexed(width, height, scan, 1) { ColorTable = colorTable };
            }
            else if (format == PixelFormat.Format4bppIndexed)
            {
                return new ScanProcessorIndexed(width, height, scan, 4) { ColorTable = colorTable };
            }
            else if (format == PixelFormat.Format8bppIndexed)
            {
                return new ScanProcessorIndexed(width, height, scan, 8) { ColorTable = colorTable };
            }
            else if (format == PixelFormat.Format16bppRgb555)
            {
                return new ScanProcessor16Rgb555(width, height, scan);
            }
            else if (format == PixelFormat.Format24bppRgb)
            {
                return new ScanProcessor24Rgb(width, height, scan);
            }
            else if (format == PixelFormat.Format32bppArgb)
            {
                return new ScanProcessor32Argb(width, height, scan);
            }
            else
            {
                throw new ArgumentException($"Unknown PixelFormat : {format}");
            }

        }

        public Bitmap ToBitmap()
        {
            unsafe
            {
                fixed (byte* scan0 = this.Scan)
                {
                    var bitmap = new Bitmap(this.Width, this.Height, this.Stride, this.Format, (IntPtr)scan0);
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
