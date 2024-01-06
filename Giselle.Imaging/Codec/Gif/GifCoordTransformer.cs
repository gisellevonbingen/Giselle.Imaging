using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging;
using Giselle.Imaging.Drawable;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifCoordTransformer : ICoordTransformer
    {
        public ImageArgb32Frame PrevFrame { get; }
        public GifFrame Frame { get; }

        public GifCoordTransformer(ImageArgb32Frame prevFrame, GifFrame frame)
        {
            this.PrevFrame = prevFrame;
            this.Frame = frame;
        }

        public PointI Encode(ScanData scanData, PointI coord) => new(coord.X - this.Frame.X, coord.Y - this.Frame.Y);

        public PointI Decode(ScanData scanData, PointI coord) => new(coord.X + this.Frame.X, coord.Y + this.Frame.Y);

    }

}
