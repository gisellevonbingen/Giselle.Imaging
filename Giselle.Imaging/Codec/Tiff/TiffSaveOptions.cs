using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Exif;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffSaveOptions : SaveOptions
    {
        public bool ExifLittleEndian { get; set; }
        public TiffFrameSaveOptions FallbackFrameOptions { get; set; }

        public TiffSaveOptions()
        {
            this.ExifLittleEndian = BitConverter.IsLittleEndian;
            this.FallbackFrameOptions = null;
        }

        public TiffSaveOptions(TiffSaveOptions other) : base(other)
        {
            this.ExifLittleEndian = other.ExifLittleEndian;
            this.FallbackFrameOptions = other.FallbackFrameOptions == null ? null : new TiffFrameSaveOptions(other.FallbackFrameOptions);
        }

    }

}
