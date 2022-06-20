using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Exif;
using Giselle.Imaging.Codec.TIff;
using Giselle.Imaging.Collections;
using Giselle.Imaging.IO;
using Giselle.Imaging.Scan;

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
            var sampleCount = subFile.SamplesPerPixel;
            var bitsPerPixel = subFile.BitsPerPixel;
            var stride = ScanProcessor.GetBytesPerWidth(subFile.Width, bitsPerPixel);
            var scan = new ScanData(subFile.Width, subFile.Height, bitsPerPixel) { Stride = stride, Scan = new byte[stride * subFile.Height], ColorTable = subFile.ColorMap };

            var offsets = subFile.StripOffsets;
            var counts = subFile.StripByteCounts;
            var predictor = subFile.Predictor;
            var rows = subFile.RowsPerStrip;
            var photometricInterpretation = subFile.PhotometricInterpretation;

            for (var i = 0; i < offsets.Length; i++)
            {
                var offset = offsets[i];
                var count = counts[i];
                input.Position = offset + origin;

                if (subFile.Compression == ExifCompressionMethod.LZW)
                {
                    using (var lzw = new ExifLZWStream(input, ExifLZWCompressionMode.Decompress, true))
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
                                else if (predictor == ExifPredictor.NoPrediction)
                                {
                                    samples[sampleIndex] = (byte)sample;
                                }
                                else if (predictor == ExifPredictor.HorizontalDifferencing)
                                {
                                    samples[sampleIndex] = (byte)(samples[sampleIndex] + sample);
                                }
                                else
                                {

                                }

                                var scanIndex = (rows * stride * i) + (stride * j) + k;
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

            var processor = subFile.GetScanProcessor();
            return new ImageArgb32Frame(scan, processor) { PrimaryCodec = this, PrimaryOptions = new TiffSaveOptions() { } };
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions options)
        {
            throw new NotImplementedException();
        }

    }

}
