using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Exif;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffSaveOptions : ExifSaveOptions
    {
        public TiffSaveOptions()
        {
        }

        public TiffSaveOptions(TiffSaveOptions other) : base(other)
        {

        }

    }

}
