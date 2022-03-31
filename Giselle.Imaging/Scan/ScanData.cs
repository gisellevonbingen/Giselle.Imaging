﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Scan
{
    public class ScanData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride { get; set; }
        public int BitsPerPixel { get; set; }
        public byte[] Scan { get; set; }
        public Argb32[] ColorTable { get; set; } = new Argb32[0];

        public ScanData(int width, int height, int stride, int bitsPerPixel)
            : this(width, height, stride, bitsPerPixel, null)
        {

        }

        public ScanData(int width, int height, int stride, int bitsPerPixel, byte[] scan)
        {
            this.Width = width;
            this.Height = height;
            this.Stride = stride;
            this.BitsPerPixel = bitsPerPixel;
            this.Scan = scan ?? new byte[height * stride];
        }

    }

}
