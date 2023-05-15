using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formats.Exif;
using Giselle.Imaging.Collections;
using Giselle.Imaging.Formats.Exif;
using Giselle.Imaging.Physical;
using Giselle.Imaging.Scan;
using Ionic.Zlib;
using Streams.IO;
using static Giselle.Imaging.ImageArgb32Frame;

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

        public override IEnumerable<string> GetExtensions()
        {
            yield return "tiff";
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
            using var siphonBlock = SiphonBlock.ByRemain(input);
            var exif = new ExifContainer(siphonBlock.SiphonSteam);
            var container = new ImageArgb32Container()
            {
                PrimaryCodec = this,
                PrimaryOptions = new TiffSaveOptions()
                {
                    ExifLittleEndian = exif.WasLittleEndian,
                },
            };

            foreach (var directory in exif.Directories)
            {
                var frame = this.Decode(siphonBlock.SiphonSteam, directory);

                if (frame != null)
                {
                    container.Add(frame);
                }

            }

            return container;
        }

        private static Argb32[] DecodeColorMap(ExifImageFileDirectory directory) => directory.TryGetIntegers(ExifTagId.ColorMap, out var integers) ? DecodeColorMap(integers) : Array.Empty<Argb32>();

        private static Argb32[] DecodeColorMap(IExifValueIntegers intgers)
        {
            var array = intgers.AsUnsigneds.ToArray();
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
                    var sampleRaw = array[arrayIndex++];
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

            return colorMap;
        }

        private static void EncodeColorMap(ExifImageFileDirectory directory, Argb32[] colorTable) => directory.SetShorts(ExifTagId.ColorMap, EncodeColorMap(colorTable));

        private static ushort[] EncodeColorMap(Argb32[] colorTable)
        {
            var arrayIndex = 0;
            var channels = 3;
            var samples = new ushort[colorTable.Length * channels];

            for (var i = 0; i < channels; i++)
            {
                for (var j = 0; j < colorTable.Length; j++)
                {
                    byte sample = 0;

                    if (i == 0)
                    {
                        sample = colorTable[j].R;
                    }
                    else if (i == 1)
                    {
                        sample = colorTable[j].G;
                    }
                    else if (i == 2)
                    {
                        sample = colorTable[j].B;
                    }

                    var sampleRaw = (ushort)((sample / 255.0D) * 65535.0D);
                    samples[arrayIndex++] = sampleRaw;
                }

            }

            return samples;
        }

        private ImageArgb32Frame Decode(Stream input, ExifImageFileDirectory directory)
        {
            if (directory.TryGetValue(ExifTagId.NewSubfileType, out var newSubfileType) == false)
            {
                return null;
            }

            var bitsPerSamples = directory.GetSigneds(ExifTagId.BitsPerSample);
            var bitsPerPixel = bitsPerSamples.Sum();
            var width = directory.GetSigned(ExifTagId.ImageWidth);
            var height = directory.GetSigned(ExifTagId.ImageLength);
            var stride = ScanProcessor.GetBytesPerWidth(width, bitsPerPixel);
            var colorMap = DecodeColorMap(directory);
            var scan = new ScanData(width, height, bitsPerPixel) { Stride = stride, Scan = new byte[stride * height], ColorTable = colorMap };

            var samplesPerPixel = directory.GetSigned(ExifTagId.SamplesPerPixel);
            var stripOffsets = directory.GetSigneds(ExifTagId.StripOffsets);
            var stripBytes = directory.GetSigneds(ExifTagId.StripByteCounts);
            var photometricInterpretation = (ExifPhotometricInterpretation)directory.GetUnsigned(ExifTagId.PhotometricInterpretation);
            var compression = (TiffCompressionMethod)directory.GetUnsigned(ExifTagId.Compression);
            var predictor = (ExifPredictor)directory.GetSigned(ExifTagId.Predictor);
            var rowsPerStrip = directory.GetSigned(ExifTagId.RowsPerStrip);

            for (var i = 0; i < stripOffsets.Length; i++)
            {
                var offset = stripOffsets[i];
                var bytes = stripBytes[i];
                input.Position = offset;

                using var ds = CreateDecompressStream(input, compression, i, bytes, true);
                Decompress(stride, samplesPerPixel, predictor, rowsPerStrip, scan.Scan, i, ds);
            }

            var processor = GetScanProcessor(photometricInterpretation, bitsPerSamples, bitsPerPixel);
            var frame = new ImageArgb32Frame(scan, processor)
            {
                PrimaryCodec = this,
                PrimaryOptions = new TiffFrameSaveOptions()
                {
                    BitsPerSample = bitsPerSamples[0],
                    SamplesPerPixel = samplesPerPixel,
                    Compression = compression,
                    PhotometricInterpretation = photometricInterpretation,
                },
            };

            var resolutionUnit = (ExifResolutionUnit)directory.GetSigned(ExifTagId.ResolutionUnit);

            if (resolutionUnit != ExifResolutionUnit.Undefined)
            {
                if (directory.TryGetValue(ExifTagId.XResolution, out var _x) && _x is ExifValueRationals xResolution)
                {
                    frame.WidthResoulution = new PhysicalDensity(xResolution.Value.Ratio, resolutionUnit.ToPhysicalUnit());
                }

                if (directory.TryGetValue(ExifTagId.YResolution, out var _y) && _y is ExifValueRationals yResolution)
                {
                    frame.HeightResoulution = new PhysicalDensity(yResolution.Value.Ratio, resolutionUnit.ToPhysicalUnit());
                }

            }

            return frame;
        }

        private static Stream CreateDecompressStream(Stream input, TiffCompressionMethod compression, int stripIndex, int stripLength, bool leaveOpen)
        {
            if (compression == TiffCompressionMethod.Undefined || compression == TiffCompressionMethod.NoCompression)
            {
                return new SiphonStream(input, stripLength, true, leaveOpen);
            }
            else if (compression == TiffCompressionMethod.T4Encoding)
            {
                throw new NotImplementedException();
            }
            else if (compression == TiffCompressionMethod.LZW)
            {
                return new TiffLZWStream(input, TiffLZWCompressionMode.Decompress, leaveOpen);
            }
            else if (compression == TiffCompressionMethod.Deflate)
            {
                return new ZlibStream(input, CompressionMode.Decompress, leaveOpen);
            }
            else
            {
                throw new NotSupportedException($"Not Supported Compression Method : {compression}");
            }

        }

        private static void Decompress(int stride, int samplesPerPixel, ExifPredictor predictor, int rowsPerStrip, byte[] scan, int stripIndex, Stream input)
        {
            for (var i = 0; i < rowsPerStrip; i++)
            {
                var prevSamples = new byte[samplesPerPixel];

                for (var x = 0; x < stride; x++)
                {
                    var sampleIndex = x % samplesPerPixel;
                    var scanIndex = (rowsPerStrip * stride * stripIndex) + (stride * i) + x;

                    var predictedSample = input.ReadByte();
                    byte originalSample;

                    if (predictedSample == -1)
                    {
                        break;
                    }
                    else if (predictor == ExifPredictor.Undefined || predictor == ExifPredictor.NoPrediction)
                    {
                        originalSample = (byte)predictedSample;
                    }
                    else if (predictor == ExifPredictor.HorizontalDifferencing)
                    {
                        originalSample = (byte)(prevSamples[sampleIndex] + predictedSample);
                    }
                    else
                    {
                        throw new InvalidDataException($"Unknown Predictor : {predictor}");
                    }

                    scan[scanIndex] = originalSample;
                    prevSamples[sampleIndex] = originalSample;
                }

            }

        }

        private static ScanProcessor GetScanProcessor(ExifPhotometricInterpretation photometricInterpretation, int[] bitsPerSample, int bitsPerPixel)
        {
            if (photometricInterpretation == ExifPhotometricInterpretation.BlackIsZero)
            {
                return ScanProcessorIndexed.BlackIsZero;
            }
            else if (photometricInterpretation == ExifPhotometricInterpretation.WhiteIsZero)
            {
                return ScanProcessorIndexed.WhiteIsZero;
            }
            else if (photometricInterpretation == ExifPhotometricInterpretation.Rgb)
            {
                var r = ((1u << bitsPerSample[0]) - 1) << 0x00;
                var g = ((1u << bitsPerSample[1]) - 1) << 0x08;
                var b = ((1u << bitsPerSample[2]) - 1) << 0x10;
                var a = bitsPerSample.Length > 3 ? ((1u << bitsPerSample[3]) - 1) << 0x18 : 0;
                return ScanProcessor.CreateScanProcessor(bitsPerPixel, a, r, g, b);
            }
            else if (photometricInterpretation == ExifPhotometricInterpretation.PaletteColor)
            {
                return ScanProcessorIndexed.Instance;
            }
            else
            {
                throw new IndexOutOfRangeException($"Unknown PhotometricInterpretation : {photometricInterpretation}");
            }

        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions _options)
        {
            var options = _options.CastOrDefault<TiffSaveOptions>();
            var exif = new ExifContainer();
            var multiPage = container.Count > 1;
            var frameOptionsMap = new Dictionary<ImageArgb32Frame, TiffFrameSaveOptions>();

            foreach (var frame in container)
            {
                PreferredIndexedReport report = null;
                var frameOptions = frameOptionsMap[frame] = frame.PrimaryOptions.CastOr(options.FallbackFrameOptions).CastOr(() =>
                {
                    report = frame.GetPreferredIndexedPixelFormat(false, this.GetSupportIndexedPixelFormats());
                    return new TiffFrameSaveOptions()
                    {
                        PhotometricInterpretation = report.IndexedPixelFormat == PixelFormat.Undefined ? ExifPhotometricInterpretation.Rgb : ExifPhotometricInterpretation.PaletteColor,
                        SamplesPerPixel = report.IndexedPixelFormat == PixelFormat.Undefined ? (report.HasAlpha ? 4 : 3) : 1,
                        BitsPerSample = report.IndexedPixelFormat == PixelFormat.Undefined ? 8 : report.IndexedPixelFormat.GetBitsPerPixel(),
                        Compression = TiffCompressionMethod.LZW,
                    };
                });
                var bitsPerPixel = frameOptions.BitsPerSample * frameOptions.SamplesPerPixel;
                var rowsPerStrip = Math.Min(Math.Max(10000 / ScanProcessor.GetBytesPerWidth(frame.Width, bitsPerPixel), 1), frame.Height);
                var strips = ScanProcessor.GetPaddedQuotient(frame.Height, rowsPerStrip);

                var directory = new ExifImageFileDirectory();
                directory.SetLong(ExifTagId.NewSubfileType, (uint)(multiPage ? ExifSubfileTypeFlag.SinglePageOfMultiPage : ExifSubfileTypeFlag.None));
                directory.SetShorts(ExifTagId.BitsPerSample, Enumerable.Repeat((ushort)frameOptions.BitsPerSample, frameOptions.SamplesPerPixel).ToArray());
                directory.SetLong(ExifTagId.ImageWidth, (uint)frame.Width);
                directory.SetLong(ExifTagId.ImageLength, (uint)frame.Height);
                directory.SetShort(ExifTagId.Compression, (ushort)frameOptions.Compression);
                directory.SetShort(ExifTagId.PhotometricInterpretation, (ushort)frameOptions.PhotometricInterpretation);
                directory.SetShort(ExifTagId.SamplesPerPixel, (ushort)frameOptions.SamplesPerPixel);
                directory.SetLong(ExifTagId.RowsPerStrip, (uint)rowsPerStrip);

                var unit = PhysicalUnit.Inch;
                var denominator = 1000u;
                directory.SetRational(ExifTagId.XResolution, new ExifRational((uint)(frame.WidthResoulution.GetConvertValue(unit) * denominator), denominator));
                directory.SetRational(ExifTagId.YResolution, new ExifRational((uint)(frame.HeightResoulution.GetConvertValue(unit) * denominator), denominator));
                directory.SetShort(ExifTagId.ResolutionUnit, (ushort)unit.ToExifResolutionUnit());
                directory.SetShort(ExifTagId.Predictor, (ushort)(frameOptions.Compression != TiffCompressionMethod.NoCompression && frameOptions.PhotometricInterpretation == ExifPhotometricInterpretation.Rgb ? ExifPredictor.HorizontalDifferencing : ExifPredictor.NoPrediction));

                directory.SetLongs(ExifTagId.StripOffsets, Enumerable.Repeat(0u, strips).ToArray());
                directory.SetLongs(ExifTagId.StripByteCounts, Enumerable.Repeat(0u, strips).ToArray());

                if (frameOptions.PhotometricInterpretation == ExifPhotometricInterpretation.PaletteColor)
                {
                    if (report == null)
                    {
                        report = frame.GetPreferredIndexedPixelFormat(false, this.GetSupportIndexedPixelFormats());
                    }

                    var colorMap = report.UniqueColors.TakeFixSize(0, PixelFormatUtils.GetColorTableLength(frameOptions.BitsPerSample)).ToArray();
                    EncodeColorMap(directory, colorMap);
                }

                exif.Directories.Add(directory);
            }

            var stripCursor = exif.InfoWithValuesSize;

            for (var i = 0; i < exif.Directories.Count; i++)
            {
                var directory = exif.Directories[i];
                var frame = container[i];
                var srtipCount = directory[ExifTagId.StripByteCounts].RawValueCount;
                var newStripOffsets = new uint[srtipCount];
                var newStripBytes = new uint[srtipCount];

                Process(directory, frame, frameOptionsMap[frame], () => new LengthOnlyStream(), (stripIndex, baseStream) =>
                {
                    newStripOffsets[stripIndex] = (uint)stripCursor;
                    newStripBytes[stripIndex] = (uint)baseStream.Length;
                    stripCursor += newStripBytes[stripIndex];
                });

                directory.SetLongs(ExifTagId.StripOffsets, newStripOffsets);
                directory.SetLongs(ExifTagId.StripByteCounts, newStripBytes);
            }

            exif.Write(output, options.ExifLittleEndian);

            for (var i = 0; i < exif.Directories.Count; i++)
            {
                var directory = exif.Directories[i];
                var frame = container[i];
                Process(directory, frame, frameOptionsMap[frame], () => output, null);
            }

        }

        private static void Process(ExifImageFileDirectory directory, ImageArgb32Frame frame, TiffFrameSaveOptions options, Func<Stream> streamProvider, Action<int, Stream> callback)
        {
            var bitsPerSamples = directory.GetSigneds(ExifTagId.BitsPerSample);
            var bitsPerPixel = bitsPerSamples.Sum();
            var width = directory.GetSigned(ExifTagId.ImageWidth);
            var height = directory.GetSigned(ExifTagId.ImageLength);
            var photometricInterpretation = (ExifPhotometricInterpretation)directory.GetUnsigned(ExifTagId.PhotometricInterpretation);

            var compression = (TiffCompressionMethod)directory.GetUnsigned(ExifTagId.Compression);
            var stride = ScanProcessor.GetBytesPerWidth(width, bitsPerPixel);
            var samplesPerPixel = directory.GetSigned(ExifTagId.SamplesPerPixel);
            var predictor = (ExifPredictor)directory.GetSigned(ExifTagId.Predictor);
            var rowsPerStrip = directory.GetSigned(ExifTagId.RowsPerStrip);
            var srtipCount = directory[ExifTagId.StripByteCounts].RawValueCount;

            var colorMap = DecodeColorMap(directory);
            var scan = new ScanData(width, height, bitsPerPixel) { Stride = stride, Scan = new byte[stride * height], ColorTable = colorMap };
            var processor = GetScanProcessor(photometricInterpretation, bitsPerSamples, bitsPerPixel);
            processor.Encode(scan, frame);

            for (var stripIndex = 0; stripIndex < srtipCount; stripIndex++)
            {
                var baseStream = streamProvider();

                using (var stream = CreateCompressStream(baseStream, compression, options.CompressionLevel, true))
                {
                    Compress(stride, samplesPerPixel, predictor, rowsPerStrip, scan.Scan, stripIndex, stream);
                }

                callback?.Invoke(stripIndex, baseStream);
            }

        }

        private static Stream CreateCompressStream(Stream output, TiffCompressionMethod compression, CommonCompressionLevel level, bool leaveOpen)
        {
            if (compression == TiffCompressionMethod.Undefined || compression == TiffCompressionMethod.NoCompression)
            {
                return new WrappedStream(output, leaveOpen);
            }
            else if (compression == TiffCompressionMethod.T4Encoding)
            {
                throw new NotImplementedException();
            }
            else if (compression == TiffCompressionMethod.LZW)
            {
                return new TiffLZWStream(output, TiffLZWCompressionMode.Compress, leaveOpen);
            }
            else if (compression == TiffCompressionMethod.Deflate)
            {
                return new ZlibStream(output, CompressionMode.Compress, level.ToZlibCompressionLevel(), leaveOpen);
            }
            else
            {
                throw new NotSupportedException($"Not Supported Compression Method : {compression}");
            }

        }

        private static void Compress(int stride, int samplesPerPixel, ExifPredictor predictor, int rowsPerStrip, byte[] scan, int stripIndex, Stream output)
        {
            for (var i = 0; i < rowsPerStrip; i++)
            {
                var prevSamples = new byte[samplesPerPixel];

                for (var x = 0; x < stride; x++)
                {
                    var sampleIndex = x % samplesPerPixel;
                    var scanIndex = (rowsPerStrip * stride * stripIndex) + (stride * i) + x;

                    if (scanIndex >= scan.Length)
                    {
                        break;
                    }

                    var originalSample = scan[scanIndex];
                    byte predictedSample;

                    if (predictor == ExifPredictor.Undefined || predictor == ExifPredictor.NoPrediction)
                    {
                        predictedSample = originalSample;
                    }
                    else if (predictor == ExifPredictor.HorizontalDifferencing)
                    {
                        predictedSample = (byte)(originalSample - prevSamples[sampleIndex]);
                    }
                    else
                    {
                        throw new InvalidDataException($"Unknown Predictor : {predictor}");
                    }

                    output.WriteByte(predictedSample);
                    prevSamples[sampleIndex] = originalSample;
                }

            }

        }

        public override IEnumerable<PixelFormat> GetSupportIndexedPixelFormats()
        {
            yield return PixelFormat.Format1bppIndexed;
            yield return PixelFormat.Format4bppIndexed;
            yield return PixelFormat.Format8bppIndexed;
        }

        public override PixelFormat GetPreferredPixelFormat(ImageArgb32Frame frame)
        {
            throw new NotSupportedException();
        }

    }

}
