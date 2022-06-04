using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffImageFileDirectory
    {
        public List<TiffEntry> Entries { get; } = new List<TiffEntry>();

        public ImageArgb32 Decode()
        {
            var subFile = new TiffSubfile();

            foreach (var entry in this.Entries)
            {
                this.ReadEntry(subFile, entry);
            }

            return null;
        }

        private void ReadEntry(TiffSubfile subFile, TiffEntry entry)
        {
            var tagId = entry.TagId;
            var value = entry.Value;
            Console.WriteLine($"{entry}");

            if (tagId == TiffTagId.NewSubfileType)
            {
                subFile.Flags = (TiffNewSubfileTypeFlag)value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.ImageWidth)
            {
                subFile.Width = value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.ImageLength)
            {
                subFile.Height = value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.BitsPerSample)
            {
                subFile.BitsPerSample = value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.SamplesPerPixel)
            {
                subFile.SamplesPerPixel = value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.Compression)
            {
                subFile.Compression = (TiffCompressionMethod)value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.PhotometricInterpretation)
            {
                subFile.PhotometricInterpretation = (TiffPhotometricInterpretation)value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.StripOffsets)
            {
                subFile.StripOffsets = value.AsNumbers().AsSigneds.ToArray();
            }
            else if (tagId == TiffTagId.RowsPerStrip)
            {
                subFile.RowsPerStrip = value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.StripByteCounts)
            {
                subFile.StripByteCounts = value.AsNumbers().AsSigneds.ToArray();
            }
            else if (tagId == TiffTagId.XResolution)
            {
                subFile.XResolution = value.AsRtaionals().Value;
            }
            else if (tagId == TiffTagId.YResolution)
            {
                subFile.YResolution = value.AsRtaionals().Value;
            }
            else if (tagId == TiffTagId.ResolutionUnit)
            {
                subFile.ResolutionUnit = (TiffResolutionUnit)value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.Software)
            {
                subFile.Software = value.AsASCII().Value;
            }
            else if (tagId == TiffTagId.Predictor)
            {
                subFile.Predictor = (TiffPredictor)value.AsNumbers().AsSigned;
            }
            else if (tagId == TiffTagId.ColorMap)
            {
                var array = value.AsNumbers().AsSigneds.ToArray();
                var divisor = 3;

                for (var i = 0; i < array.Length; i += divisor)
                {
                    var offset = i * divisor;
                    var r = (ushort)array[i + 0];
                    var g = (ushort)array[i + 1];
                    var b = (ushort)array[i + 2];
                }

            }

        }

    }

}
