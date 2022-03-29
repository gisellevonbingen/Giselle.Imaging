using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public struct ColorWithPosition
    {
        public ImageArgb32 Image { get; }
        public int X { get; }
        public int Y { get; }

        public ColorWithPosition(ImageArgb32 image, int x, int y) : this()
        {
            this.Image = image;
            this.X = x;
            this.Y = y;
        }

        public Argb32 Color
        {
            get => this.Image[this.X, this.Y];
            set => this.Image[this.X, this.Y] = value;
        }

    }

}
