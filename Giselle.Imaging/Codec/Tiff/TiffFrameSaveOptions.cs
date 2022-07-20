using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Exif;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffFrameSaveOptions : SaveOptions
    {
        public int BitsPerSample { get; set; }
        public int SamplesPerPixel { get; set; }
        public TiffCompressionMethod Compression { get; set; }
        public ExifPhotometricInterpretation PhotometricInterpretation { get; set; }

        public TiffFrameSaveOptions()
        {
        }

        public TiffFrameSaveOptions(TiffFrameSaveOptions other) : base(other)
        {
            this.BitsPerSample = other.BitsPerSample;
            this.SamplesPerPixel = other.SamplesPerPixel;
            this.Compression = other.Compression;
            this.PhotometricInterpretation = other.PhotometricInterpretation;
        }

    }

}
