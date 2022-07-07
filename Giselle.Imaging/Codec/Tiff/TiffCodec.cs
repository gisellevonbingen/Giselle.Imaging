using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Exif;
using Giselle.Imaging.Codec.TIff;
using Giselle.Imaging.Scan;
using Ionic.Zlib;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffCodec : ImageCodec
    {
        public static TiffCodec Instance { get; } = new TiffCodec();

        public TiffCodec()
        {

        }

        public override int BytesForTest => ExifContainer.SignatureLength + 2;

        public override bool SupportMultiFrame => false;

        public override string PrimaryExtension => "tiff";

        public override IEnumerable<string> GetExtensions()
        {
            yield return PrimaryExtension;
            yield return "tif";
        }

        protected override bool TestAsStream(Stream stream)
        {
            var processor = ExifContainer.CreateExifProcessor(stream);
            var signature = processor.ReadBytes(ExifContainer.SignatureLength);

            if (ExifContainer.TryGetEndian(signature, out var isLittleEndian) == false)
            {
                return false;
            }

            processor.IsLittleEndian = isLittleEndian;
            var endianChecker = processor.ReadShort();
            return endianChecker == ExifContainer.EndianChecker;
        }

        public override ImageArgb32Container Read(Stream input)
        {
            if (input.CanSeek == false)
            {
                throw new ArgumentException("Tiff input stream required be seekable");
            }

            var origin = input.Position;
            var exif = new ExifContainer(input, origin);
            var container = new ImageArgb32Container()
            {
                PrimaryCodec = this,
                PrimaryOptions = new TiffSaveOptions(),
            };

            foreach (var directory in exif.Directories)
            {
                var frame = this.Decode(input, directory, origin);

                if (frame != null)
                {
                    container.Add(frame);
                }

            }

            return container;
        }

        private ImageArgb32Frame Decode(Stream input, ExifImageFileDirectory directory, long origin)
        {
            var newSubfileType = directory.Entries.FirstOrDefault(e => e.TagId == ExifTagId.NewSubfileType);

            if (newSubfileType == null)
            {
                return null;
            }

            var subFile = new TiffSubfile(directory.Entries);
            var bitsPerPixel = subFile.BitsPerPixel;
            var stride = ScanProcessor.GetBytesPerWidth(subFile.Width, bitsPerPixel);
            var scan = new ScanData(subFile.Width, subFile.Height, bitsPerPixel) { Stride = stride, Scan = new byte[stride * subFile.Height], ColorTable = subFile.ColorMap };

            var offsets = subFile.StripOffsets;
            var counts = subFile.StripByteCounts;
            var photometricInterpretation = subFile.PhotometricInterpretation;

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var count = counts[i];
                var compression = subFile.Compression;
                input.Position = offset + origin;

                using (var ds = CreateDecompressStream(input, compression, true))
                {
                    this.Decompress(subFile, stride, scan.Scan, i, ds);
                }

            }

            var processor = subFile.GetScanProcessor();
            return new ImageArgb32Frame(scan, processor) { PrimaryCodec = this, PrimaryOptions = new TiffSaveOptions() { } };
        }

        private Stream CreateDecompressStream(Stream input, ExifCompressionMethod compression, bool leavOpen)
        {
            if (compression == ExifCompressionMethod.LZW)
            {
                return new ExifLZWStream(input, ExifLZWCompressionMode.Decompress, leavOpen);
            }
            else if (compression == ExifCompressionMethod.Deflate)
            {
                return new ZlibStream(input, CompressionMode.Decompress, leavOpen);
            }
            else
            {
                throw new NotSupportedException($"Not Supported Compression Method : {compression}");
            }

        }

        private void Decompress(TiffSubfile subFile, int stride, byte[] scan, int stripIndex, Stream input)
        {
            var sampleCount = subFile.SamplesPerPixel;
            var predictor = subFile.Predictor;
            var rows = subFile.RowsPerStrip;

            for (var j = 0; j < rows; j++)
            {
                var samples = new byte[sampleCount];

                for (var k = 0; k < stride; k++)
                {
                    var sampleIndex = k % sampleCount;
                    var sample = input.ReadByte();

                    if (sample == -1)
                    {
                        break;
                    }
                    else if (predictor == ExifPredictor.Undefined || predictor == ExifPredictor.NoPrediction)
                    {
                        samples[sampleIndex] = (byte)sample;
                    }
                    else if (predictor == ExifPredictor.HorizontalDifferencing)
                    {
                        samples[sampleIndex] = (byte)(samples[sampleIndex] + sample);
                    }
                    else
                    {
                        throw new InvalidDataException($"Unknown Predictor : {predictor}");
                    }

                    var scanIndex = (rows * stride * stripIndex) + (stride * j) + k;
                    scan[scanIndex] = samples[sampleIndex];
                    //Console.WriteLine(scanIndex);
                }

            }
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions options)
        {
            throw new NotImplementedException();
        }

    }

}
