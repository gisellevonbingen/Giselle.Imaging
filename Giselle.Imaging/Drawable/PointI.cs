using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Drawable
{
    public struct PointI : IEquatable<PointI>
    {
        public static bool operator ==(PointI o1, PointI o2) => o1.Equals(o2) == true;

        public static bool operator !=(PointI o1, PointI o2) => o1.Equals(o2) == false;

        public static PointI operator +(PointI o1, PointI o2) => new PointI(o1.X + o2.X, o1.Y + o2.Y);

        public static PointI operator -(PointI o1, PointI o2) => new PointI(o1.X - o2.X, o1.Y - o2.Y);

        public int X { get; set; }
        public int Y { get; set; }

        public PointI(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"X:{this.X}, Y:{this.Y}";

        public override bool Equals(object obj) => obj is PointI other && this.Equals(other);

        public bool Equals(PointI other) => this.X == other.X && this.Y == other.Y;
    }

}
