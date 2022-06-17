using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Bmp;
using Giselle.Imaging.Codec.Png;
using Giselle.Imaging.Drawable;

namespace Giselle.Imaging.Codec.Ico
{
    public class IcoSaveOptions : SaveOptions
    {
        public IcoImageType Type { get; set; } = IcoImageType.Icon;
        public PointI[] CursorHotspots { get; set; } = new PointI[0];
    }

}
