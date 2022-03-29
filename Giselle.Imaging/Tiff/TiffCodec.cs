using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Tiff
{
    public class TiffCodec : ImageCodec<TiffEncodeOptions>
    {
        public static TiffCodec Instance { get; } = new TiffCodec();

        public static IList<byte> SignatureLittleEndian { get; } = Array.AsReadOnly(new byte[] { 0x49, 0x49 });
        public static IList<byte> SignatureBigEndian { get; } = Array.AsReadOnly(new byte[] { 0x4D, 0x4D });
        public static IList<IList<byte>> Signatures { get; } = Array.AsReadOnly(new IList<byte>[] { SignatureLittleEndian, SignatureBigEndian });

        public const short EndianChecker = 0x002A;

        public static DataProcessor CreateTiffProcessor(Stream stream) => new DataProcessor(stream) { };

        public TiffCodec()
        {

        }

        public override int BytesForTest => 2;

        public override bool Test(byte[] bytes) => Signatures.Any(s => bytes.StartsWith(s));

        public override ImageArgb32 Read(Stream input)
        {
            var processor = CreateTiffProcessor(input);
            // Signature
            var signature = processor.ReadBytes(BytesForTest);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            processor.IsLittleEndian = this.IsLittleEndian(signature);
            var endianChecker = processor.ReadShort();

            if (endianChecker != EndianChecker)
            {
                throw new IOException($"Endian Check Failed : Read={endianChecker:X4}, Require={EndianChecker:X4}");
            }

            var ifdOffset = processor.ReadInt();

            while (true)
            {
                if (ifdOffset == 0)
                {
                    break;
                }

                var o = processor.ReadLength;
                processor.SkipByRead(ifdOffset - o);

                var subFile = new TiffSubfile();
                var entryCount = processor.ReadShort();
                var entries = new List<TiffEntry>();

                for (var i = 0; i < entryCount; i++)
                {
                    var entry = new TiffEntry();
                    entry.Read(processor);
                    entries.Add(entry);
                }

                ifdOffset = processor.ReadInt();

                foreach (var entry in entries)
                {
                    if (entry.IsOffset == true)
                    {
                        var skipping = entry.ValueOrOffset - processor.ReadLength;

                        if (skipping < 0)
                        {
                            throw new IOException($"Entry Value Offset Matched : Require={entry.ValueOrOffset:X8}, Reading={processor.ReadLength:X8}");
                        }
                        else
                        {
                            processor.SkipByRead(skipping);
                        }

                    }

                    this.ReadEntry(entry, processor, subFile);
                }

            }

            throw new NotImplementedException();
        }

        private void ReadEntry(TiffEntry entry, DataProcessor processor, TiffSubfile subFile)
        {
            var tagId = entry.TagId;
            Console.WriteLine(entry);

            if (tagId == TiffTagId.NewSubfileType)
            {
                subFile.Flags = (TiffNewSubfileTypeFlag)entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.ImageWidth)
            {
                subFile.Width = entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.ImageLength)
            {
                subFile.Height = entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.BitsPerSample)
            {
                subFile.BitsPerSample = entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.SamplesPerPixel)
            {
                subFile.SamplesPerPixel = entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.Compression)
            {
                subFile.Compression = (TiffCompressionMethod)entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.PhotometricInterpretation)
            {
                subFile.PhotometricInterpretation = (TiffPhotometricInterpretation)entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.StripOffsets)
            {
                subFile.StripOffsets = entry.ReadAsSigneds(processor);
            }
            else if (tagId == TiffTagId.RowsPerStrip)
            {
                subFile.RowsPerStrip = entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.StripByteCounts)
            {
                subFile.StripByteCounts = entry.ReadAsSigneds(processor);
            }
            else if (tagId == TiffTagId.XResolution)
            {
                subFile.XResolution = entry.ReadAsRational(processor);
            }
            else if (tagId == TiffTagId.YResolution)
            {
                subFile.YResolution = entry.ReadAsRational(processor);
            }
            else if (tagId == TiffTagId.ResolutionUnit)
            {
                subFile.ResolutionUnit = (TiffResolutionUnit)entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.Software)
            {
                subFile.Software = entry.ReadAsASCII(processor);
            }
            else if (tagId == TiffTagId.Predictor)
            {
                subFile.Predictor = (TiffPredictor)entry.ReadAsSigned(processor);
            }
            else if (tagId == TiffTagId.ColorMap)
            {
                var array = entry.ReadAsSigneds(processor);
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

        public override void Write(Stream output, ImageArgb32 image, TiffEncodeOptions options)
        {
            throw new NotImplementedException();
        }

        public bool IsLittleEndian(byte[] signature) => SignatureLittleEndian.StartsWith(signature);

    }

}
