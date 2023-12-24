using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Codec.Ico
{
    public abstract class IcoFrame
    {
        public PointI Hotspot { get; set; }

        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract int BitsPerPixel { get; }

        public IcoFrame()
        {

        }

        public abstract void Read(Stream input, IcoImageInfo info);

        public abstract void Write(Stream output, IcoImageInfo info);

        public abstract ImageArgb32Frame Decode();

        public abstract void Encod(ImageArgb32Frame frame);
    }

}
