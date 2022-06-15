using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Giselle.Imaging.IO;
using Giselle.Imaging.Physical;
using Giselle.Imaging.Scan;
using Ionic.Zlib;

namespace Giselle.Imaging.Codec.Png
{
    public class PngCodec : ImageCodec
    {
        public const bool IsLittleEndian = false;
        public static PngCodec Instance { get; } = new PngCodec();
        public static IList<byte> Signature { get; } = Array.AsReadOnly(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        public static DataProcessor CreatePngProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = IsLittleEndian };

        public PngCodec()
        {

        }

        public override int BytesForTest => Signature.Count;

        public override bool SupportMultiFrame => false;

        public override string PrimaryExtension => "png";

        public override IEnumerable<string> GetExtensions()
        {
            yield return PrimaryExtension;
        }

        public override bool Test(byte[] bytes) => bytes.StartsWith(Signature);

        public override ImageArgb32Container Read(Stream input)
        {
            var processor = CreatePngProcessor(input);
            var signature = processor.ReadBytes(BytesForTest);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            var raw = new PngRawImage(processor);
            var width = raw.Width;
            var height = raw.Height;
            var bitsPerPixel = raw.PixelFormat.GetBitsPerPixel();
            var samples = bitsPerPixel / raw.BitDepth;
            var colorTable = ColorTableUtils.MergeColorTable(raw.RgbTable, raw.AlphaTable);
            var scanData = PngRawImage.CreateScanData(width, height, bitsPerPixel, raw.Stride, raw.Interlace, colorTable);

            raw.CompressedScanData.Position = 0L;

            using (var ds = new ZlibStream(raw.CompressedScanData, CompressionMode.Decompress, true))
            {
                var passProcessor = new InterlacePassProcessor(scanData);
                var dataProcessor = CreatePngProcessor(ds);
                var scanOffset = 0;

                while (passProcessor.NextPass() == true)
                {
                    var passInfo = passProcessor.PassInfo;
                    var scanline = new byte[passInfo.Stride];

                    for (var yi = 0; yi < passInfo.PixelsY; yi++)
                    {
                        var filter = dataProcessor.ReadByte();
                        var strideBytes = dataProcessor.ReadBytes(passInfo.Stride);
                        var currLineSamples1 = new byte[samples];
                        var lastLineSamples2 = new byte[samples];

                        for (var xi = 0; xi < passInfo.Stride; xi++)
                        {
                            var x = strideBytes[xi];
                            var a = currLineSamples1[xi % samples];
                            var b = scanline[xi];
                            var c = lastLineSamples2[xi % samples];
                            byte result = 0;

                            if (filter == 0)
                            {
                                result = x;
                            }
                            else if (filter == 1)
                            {
                                result = (byte)(x + a);
                            }
                            else if (filter == 2)
                            {
                                result = (byte)(x + b);
                            }
                            else if (filter == 3)
                            {
                                result = (byte)(x + (a + b) / 2);
                            }
                            else if (filter == 4)
                            {
                                var p = a + b - c;
                                var pa = Math.Abs(p - a);
                                var pb = Math.Abs(p - b);
                                var pc = Math.Abs(p - c);
                                if (pa <= pb && pa <= pc) result = a;
                                else if (pb <= pc) result = b;
                                else result = c;
                            }

                            scanline[xi] = result;
                            currLineSamples1[xi % samples] = result;
                            lastLineSamples2[xi % samples] = a;

                            scanData.Scan[scanOffset++] = result;
                        }

                    }

                }

            }

            var scanProcessor = raw.CreateScanProcessor();
            var densityUnit = raw.PhysicalPixelDimensionsUnit.ToDensityUnit();
            return new ImageArgb32Container(new ImageArgb32Frame(scanData, scanProcessor)
            {
                PrimaryCodec = this,
                PrimaryOptions = new PngSaveOptions() { Interlace = raw.Interlace, BitDepth = raw.BitDepth, ColorType = raw.ColorType, },
                WidthResoulution = new PhysicalDensity(raw.XPixelsPerUnit, densityUnit),
                HeightResoulution = new PhysicalDensity(raw.YPixelsPerUnit, densityUnit),
                ICCProfile = raw.ICCProfile,
            });
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions _options)
        {
            var frame = container.FirstOrDefault();
            var options = _options.CastOrDefault<PngSaveOptions>();
            var raw = new PngRawImage(frame, options);

            var processor = CreatePngProcessor(output);
            processor.WriteBytes(Signature);

            var bitPerPixel = raw.PixelFormat.GetBitsPerPixel();
            var samples = bitPerPixel / raw.BitDepth;
            raw.CompressedScanData.Position = 0L;

            var colorTable = ColorTableUtils.MergeColorTable(raw.RgbTable, raw.AlphaTable);
            var scanData = PngRawImage.CreateScanData(frame.Width, frame.Height, raw.PixelFormat.GetBitsPerPixel(), raw.Stride, raw.Interlace, colorTable);
            var scanProcessor = raw.CreateScanProcessor();
            scanProcessor.Write(scanData, frame.Scan);

            using (var ds = new ZlibStream(raw.CompressedScanData, CompressionMode.Compress, options.CompressionLevel.ToZlibCompressionLevel(), true))
            {
                var passProcessor = new InterlacePassProcessor(scanData);
                var dataProcessor = CreatePngProcessor(ds);
                var scanOffset = 0;

                while (passProcessor.NextPass() == true)
                {
                    var passInfo = passProcessor.PassInfo;
                    var scanline = new byte[passInfo.Stride];

                    for (var yi = 0; yi < passInfo.PixelsY; yi++)
                    {
                        byte filter = 0;
                        var currLineSamples1 = new byte[samples];
                        var lastLineSamples2 = new byte[samples];

                        for (var xi = 0; xi < passInfo.Stride; xi++)
                        {
                            var x = scanData.Scan[scanOffset++];
                            var a = currLineSamples1[xi % samples];
                            var b = scanline[xi];
                            var c = lastLineSamples2[xi % samples];
                            byte result = 0;

                            if (filter == 0)
                            {
                                result = x;
                            }
                            else if (filter == 1)
                            {
                                result = (byte)(x - a);
                            }
                            else if (filter == 2)
                            {
                                result = (byte)(x - b);
                            }
                            else if (filter == 3)
                            {
                                result = (byte)(x - (a + b) / 2);
                            }
                            else
                            {
                                throw new NotImplementedException($"Filter is {filter}");
                            }

                            scanline[xi] = result;
                            currLineSamples1[xi % samples] = result;
                            lastLineSamples2[xi % samples] = a;
                        }

                        dataProcessor.WriteByte(filter);
                        dataProcessor.WriteBytes(scanline);
                    }

                }

            }

            raw.Write(processor);
        }

    }

}
