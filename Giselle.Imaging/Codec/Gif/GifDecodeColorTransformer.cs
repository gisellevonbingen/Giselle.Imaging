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
    public class GifDecodeColorTransformer : IColorTransformer
    {
        public ImageArgb32Frame PrevFrame { get; }
        public GifFrame Frame { get; }

        public GifDecodeColorTransformer(ImageArgb32Frame prevFrame, GifFrame frame)
        {
            this.PrevFrame = prevFrame;
            this.Frame = frame;
        }

        public Argb32 Encode(ScanData scanData, PointI coord, int tableIndex, Argb32 color)
        {
            return color;
        }

        public Argb32 Decode(ScanData scanData, PointI coord, int tableIndex, Argb32 color)
        {
            if (this.PrevFrame != null && tableIndex == this.Frame.TransparentColorIndex)
            {
                return this.PrevFrame[coord];
            }
            else
            {
                return color;
            }

        }

    }

}
