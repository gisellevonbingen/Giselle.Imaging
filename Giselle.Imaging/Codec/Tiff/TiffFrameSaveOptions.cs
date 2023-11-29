using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formats.Exif;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffFrameSaveOptions : SaveOptions<TiffFrameSaveOptions>
    {
        public int BitsPerSample { get; set; }
        public int SamplesPerPixel { get; set; }
        public TiffCompressionMethod Compression { get; set; }
        public CommonCompressionLevel CompressionLevel { get; set; }
        public ExifPhotometricInterpretation PhotometricInterpretation { get; set; }

        public TiffFrameSaveOptions()
        {
            this.BitsPerSample = 8;
            this.SamplesPerPixel = 4;
            this.Compression = TiffCompressionMethod.Deflate;
            this.CompressionLevel = CommonCompressionLevel.Default;
            this.PhotometricInterpretation = ExifPhotometricInterpretation.Rgb;
        }

        public TiffFrameSaveOptions(TiffFrameSaveOptions other) : base(other)
        {
            this.BitsPerSample = other.BitsPerSample;
            this.SamplesPerPixel = other.SamplesPerPixel;
            this.Compression = other.Compression;
            this.CompressionLevel = other.CompressionLevel;
            this.PhotometricInterpretation = other.PhotometricInterpretation;
        }

        public override TiffFrameSaveOptions Clone() => new(this);

    }

}
