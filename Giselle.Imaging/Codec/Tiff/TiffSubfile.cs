using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffSubfile
    {
        public TiffNewSubfileTypeFlag Flags { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int[] BitsPerSample { get; set; } = new int[0];
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
        public Argb32[] ColorMap { get; set; } = new Argb32[0];

        public int BitsPerPixel => this.BitsPerSample.Sum();

        public ScanProcessor GetScanProcessor()
        {
            if (this.SamplesPerPixel == 1)
            {
                return ScanProcessorIndexed.Instance;
            }
            else
            {
                var r = ((1u << this.BitsPerSample[0]) - 1) << 0x00;
                var g = ((1u << this.BitsPerSample[1]) - 1) << 0x08;
                var b = ((1u << this.BitsPerSample[2]) - 1) << 0x10;
                var a = this.BitsPerSample.Length > 3 ?((1u << this.BitsPerSample[3]) - 1) << 0x18 : 0;
                return ScanProcessor.CreateScanProcessor(this.BitsPerPixel, a, r, g, b);
            }

        }

    }

}
