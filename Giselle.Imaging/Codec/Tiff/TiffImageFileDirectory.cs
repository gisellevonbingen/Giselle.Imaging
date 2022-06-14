using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Imaging.IO;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffImageFileDirectory
    {
        public List<TiffEntry> Entries { get; } = new List<TiffEntry>();

        public ImageArgb32 Decode(TiffRawImage raw)
        {
            var subFile = new TiffSubfile();

            foreach (var entry in this.Entries)
            {
                this.ReadEntry(raw, subFile, entry);
            }

            var sampleCount = subFile.SamplesPerPixel;
            var stride = subFile.Width * sampleCount;
            var scan = new ScanData(subFile.Width, subFile.Height, subFile.BitsPerPixel) { Stride = stride, Scan = new byte[stride * subFile.Height], ColorTable = subFile.ColorMap };

            var offsets = subFile.StripOffsets;
            var counts = subFile.StripByteCounts;
            var predictor = subFile.Predictor;
            var rows = subFile.RowsPerStrip;
            var photometricInterpretation = subFile.PhotometricInterpretation;

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var count = counts[i];

                if (raw.Strips.TryGetValue(offset, out var bytes) == true)
                {
                    using (var ms = new MemoryStream(bytes))
                    {
                        if (subFile.Compression == TiffCompressionMethod.LZW)
                        {
                            using (var lzw = new TiffLZWStream(ms, TIffLZWCompressionMode.Decompress))
                            {
                                for (var j = 0; j < rows; j++)
                                {
                                    var samples = new byte[sampleCount];

                                    for (var k = 0; k < stride; k++)
                                    {
                                        var sampleIndex = k % subFile.SamplesPerPixel;
                                        var sample = lzw.ReadByte();

                                        if (sample == -1)
                                        {
                                            break;
                                        }
                                        else if (predictor == TiffPredictor.NoPrediction)
                                        {
                                            samples[sampleIndex] = (byte)sample;
                                        }
                                        else if (predictor == TiffPredictor.HorizontalDifferencing)
                                        {
                                            samples[sampleIndex] = (byte)(samples[sampleIndex] + sample);
                                        }
                                        else
                                        {

                                        }

                                        var scanIndex = (rows * scan.Stride * i) + (scan.Stride * j) + k;
                                        scan.Scan[scanIndex] = samples[sampleIndex];
                                        //Console.WriteLine(scanIndex);
                                    }

                                }

                            }

                        }
                        else
                        {

                        }

                    }

                }
                else
                {

                }

            }

            var processor = subFile.GetScanProcessor();
            return new ImageArgb32(scan, processor);
        }

        private void ReadEntry(TiffRawImage raw, TiffSubfile subFile, TiffEntry entry)
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
                subFile.BitsPerSample = value.AsNumbers().AsSigneds.ToArray();
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

                subFile.ColorMap = colorMap;
            }

        }

    }

}
