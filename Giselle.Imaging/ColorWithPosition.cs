using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public struct ColorWithPosition
    {
        public Image32Argb Image { get; }
        public int X { get; }
        public int Y { get; }

        public ColorWithPosition(Image32Argb image, int x, int y) : this()
        {
            this.Image = image;
            this.X = x;
            this.Y = y;
        }

        public Color Color
        {
            get => this.Image[this.X, this.Y];
            set => this.Image[this.X, this.Y] = value;
        }

    }

}
