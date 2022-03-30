using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffSubfile
    {
        public TiffNewSubfileTypeFlag Flags { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int BitsPerSample { get; set; }
        public int SamplesPerPixel { get; set; }
        public TiffCompressionMethod Compression { get; set; }
        public TiffPhotometricInterpretation PhotometricInterpretation { get; set; }
        public int[] StripOffsets { get; set; } = new int[0];
        public int[] StripByteCounts { get; set; } = new int[0];
        public int RowsPerStrip { get; set; }
        public TiffRational XResolution { get; set; }
        public TiffRational YResolution { get; set; }
        public TiffResolutionUnit ResolutionUnit { get; set; }
        public string Software { get; set; } = string.Empty;
        public TiffPredictor Predictor { get; set; }
    }

}
