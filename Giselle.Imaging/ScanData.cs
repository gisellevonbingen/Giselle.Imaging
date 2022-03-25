﻿using System;
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
