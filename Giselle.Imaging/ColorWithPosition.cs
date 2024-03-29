﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public struct ColorWithPosition
    {
        public ImageArgb32Frame Frame { get; }
        public int X { get; }
        public int Y { get; }

        public ColorWithPosition(ImageArgb32Frame frame, int offset) : this()
        {
            this.Frame = frame;
            this.X = offset % frame.Width;
            this.Y = offset / frame.Width;
        }

        public Argb32 Color
        {
            get => this.Frame[this.X, this.Y];
            set => this.Frame[this.X, this.Y] = value;
        }

    }

}
