using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Exif;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffSubfile
    {
        public ExifSubfileTypeFlag Flags { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int[] BitsPerSample { get; set; } = new int[0];
        public int SamplesPerPixel { get; set; }
        public ExifCompressionMethod Compression { get; set; }
        public ExifPhotometricInterpretation PhotometricInterpretation { get; set; }
        public int[] StripOffsets { get; set; } = new int[0];
        public int[] StripByteCounts { get; set; } = new int[0];
        public int RowsPerStrip { get; set; }
        public ExifRational XResolution { get; set; }
        public ExifRational YResolution { get; set; }
        public ExifResolutionUnit ResolutionUnit { get; set; }
        public string Software { get; set; } = string.Empty;
        public ExifPredictor Predictor { get; set; }
        public Argb32[] ColorMap { get; set; } = new Argb32[0];

        public int BitsPerPixel => this.BitsPerSample.Sum();

        public TiffSubfile()
        {

        }

        public TiffSubfile(IEnumerable<ExifEntry> entries)
        {
            foreach (var entry in entries)
            {
                this.ReadEntry(entry);
            }

        }

        public void ReadEntry(ExifEntry entry)
        {
            var tagId = entry.TagId;
            var value = entry.Value;
            Console.WriteLine($"{entry}");

            if (tagId == ExifTagId.NewSubfileType)
            {
                this.Flags = (ExifSubfileTypeFlag)value.AsNumbers().AsSigned;
            }
            else if (tagId == ExifTagId.ImageWidth)
            {
                this.Width = value.AsNumbers().AsSigned;
            }
            else if (tagId == ExifTagId.ImageLength)
            {
                this.Height = value.AsNumbers().AsSigned;
            }
            else if (tagId == ExifTagId.BitsPerSample)
            {
                this.BitsPerSample = value.AsNumbers().AsSigneds.ToArray();
            }
            else if (tagId == ExifTagId.SamplesPerPixel)
            {
                this.SamplesPerPixel = value.AsNumbers().AsSigned;
            }
            else if (tagId == ExifTagId.Compression)
            {
                this.Compression = (ExifCompressionMethod)value.AsNumbers().AsSigned;
            }
            else if (tagId == ExifTagId.PhotometricInterpretation)
            {
                this.PhotometricInterpretation = (ExifPhotometricInterpretation)value.AsNumbers().AsSigned;
            }
            else if (tagId == ExifTagId.StripOffsets)
            {
                this.StripOffsets = value.AsNumbers().AsSigneds.ToArray();
            }
            else if (tagId == ExifTagId.RowsPerStrip)
            {
                this.RowsPerStrip = value.AsNumbers().AsSigned;
            }
            else if (tagId == ExifTagId.StripByteCounts)
            {
                this.StripByteCounts = value.AsNumbers().AsSigneds.ToArray();
            }
            else if (tagId == ExifTagId.XResolution)
            {
                this.XResolution = value.AsRtaionals().Value;
            }
            else if (tagId == ExifTagId.YResolution)
            {
                this.YResolution = value.AsRtaionals().Value;
            }
            else if (tagId == ExifTagId.ResolutionUnit)
            {
                this.ResolutionUnit = (ExifResolutionUnit)value.AsNumbers().AsSigned;
            }
            else if (tagId == ExifTagId.Software)
            {
                this.Software = value.AsASCII().Value;
            }
            else if (tagId == ExifTagId.Predictor)
            {
                this.Predictor = (ExifPredictor)value.AsNumbers().AsSigned;
            }
            else if (tagId == ExifTagId.ColorMap)
            {
                var array = value.AsNumbers().AsSigneds.ToArray();
                var arrayIndex = 0;
                var channels = 3;
                var colorMap = new Argb32[array.Length / channels];

                for (var j = 0; j < colorMap.Length; j++)
                {
                    colorMap[j].A = byte.MaxValue;
                }

                for (var i = 0; i < channels; i++)
                {
                    for (var j = 0; j < colorMap.Length; j++)
                    {
                        var sampleRaw = (ushort)array[arrayIndex++];
                        var sample = (byte)((sampleRaw / 65535.0D) * 255);

                        if (i == 0)
                        {
                            colorMap[j].R = sample;
                        }
                        else if (i == 1)
                        {
                            colorMap[j].G = sample;
                        }
                        else if (i == 2)
                        {
                            colorMap[j].B = sample;
                        }

                    }

                }

                this.ColorMap = colorMap;
            }

        }

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
                var a = this.BitsPerSample.Length > 3 ? ((1u << this.BitsPerSample[3]) - 1) << 0x18 : 0;
                return ScanProcessor.CreateScanProcessor(this.BitsPerPixel, a, r, g, b);
            }

        }

    }

}
