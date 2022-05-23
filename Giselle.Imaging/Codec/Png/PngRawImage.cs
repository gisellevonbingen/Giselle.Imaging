using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.ICC;
using Giselle.Imaging.IO;
using Giselle.Imaging.Physical;
using Giselle.Imaging.Scan;
using Ionic.Zlib;

namespace Giselle.Imaging.Codec.Png
{
    public class PngRawImage : RawImage<PngEncodeOptions>
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte BitDepth { get; set; }
        public PngColorType ColorType { get; set; }
        public bool HasAlpha => (this.ColorType & PngColorType.WithAlphaMask) == PngColorType.WithAlphaMask;
        public byte Compression { get; set; }
        public byte Filter { get; set; }
        public byte Interlace { get; set; }
        public Argb32[] RgbTable { get; set; } = new Argb32[0];
        public byte[] AlphaTable { get; set; } = new byte[0];
        public MemoryStream CompressedScanData { get; set; } = new MemoryStream();
        public PngPhysicalPixelDimensionsUnit PhysicalPixelDimensionsUnit { get; set; }
        public int XPixelsPerUnit { get; set; }
        public int YPixelsPerUnit { get; set; }
        public List<PNGRawChunk> ExtraChunks { get; set; } = new List<PNGRawChunk>();

        public PngRawImage()
        {

        }

        public PixelFormat PixelFormat
        {
            get => PngColorTypeExtensions.ToPixelFormat(this.ColorType, this.BitDepth);
            set => (this.ColorType, this.BitDepth) = value.ToPngPixelFormat();
        }

        public static int GetStride(int width, PngColorType colorType, byte bitDepth)
        {
            var bitsPerPixel = PngColorTypeExtensions.ToPixelFormat(colorType, bitDepth).GetBitsPerPixel();
            return ScanProcessor.GetStride(width, bitsPerPixel, 1);
        }

        public int Stride => GetStride(this.Width, this.ColorType, this.BitDepth);

        public static ScanData CreateScanData(int width, int height, int bitsPerPixel, int stride, int interlace, Argb32[] colorTable)
        {
            var scanData = new ScanData(width, height, bitsPerPixel) { Stride = stride, ColorTable = colorTable };

            if (interlace == 1)
            {
                scanData.InterlaceBlockWidth = 8;
                scanData.InterlaceBlockHeight = 8;
                scanData.InterlacePasses = new[]
                {
                    new InterlacePass(0, 0, 8, 8),
                    new InterlacePass(4, 0, 8, 8),
                    new InterlacePass(0, 4, 4, 8),
                    new InterlacePass(2, 0, 4, 4),
                    new InterlacePass(0, 2, 2, 4),
                    new InterlacePass(1, 0, 2, 2),
                    new InterlacePass(0, 1, 1, 2)
                };
            }

            scanData.Scan = new byte[scanData.PreferredScanSize];
            return scanData;
        }

        public void Read(DataProcessor input)
        {
            this.CompressedScanData.Position = 0L;

            while (true)
            {
                using (var chunkStream = new PngChunkStream(input))
                {
                    try
                    {
                        this.ReadChunk(chunkStream);
                    }
                    catch (Exception)
                    {
                        chunkStream.IgnoreCRC = true;
                        throw;
                    }

                    if (chunkStream.Name.Equals(PngChunkName.IEND) == true)
                    {
                        break;
                    }

                }

            }

        }

        private void ReadChunk(PngChunkStream chunkStream)
        {
            var chunkProcessor = PngCodec.CreatePngProcessor(chunkStream);
            var type = chunkStream.Name;

            if (type.Equals(PngChunkName.IHDR) == true)
            {
                this.Width = chunkProcessor.ReadInt();
                this.Height = chunkProcessor.ReadInt();
                this.BitDepth = chunkProcessor.ReadByte();
                this.ColorType = (PngColorType)chunkProcessor.ReadByte();
                this.Compression = chunkProcessor.ReadByte();
                this.Filter = chunkProcessor.ReadByte();
                this.Interlace = chunkProcessor.ReadByte();
            }
            else if (type.Equals(PngChunkName.PLTE) == true)
            {
                var colorTable = new List<Argb32>();

                while (chunkProcessor.Position < chunkProcessor.Length)
                {
                    var r = chunkProcessor.ReadByte();
                    var g = chunkProcessor.ReadByte();
                    var b = chunkProcessor.ReadByte();
                    var color = new Argb32(r, g, b);
                    colorTable.Add(color);
                }

                this.RgbTable = colorTable.ToArray();
            }
            else if (type.Equals(PngChunkName.gAMA) == true)
            {
                var gamma = chunkProcessor.ReadInt();
            }
            else if (type.Equals(PngChunkName.cHRM) == true)
            {
                var whiteX = chunkProcessor.ReadInt();
                var whiteY = chunkProcessor.ReadInt();
                var rX = chunkProcessor.ReadInt();
                var rY = chunkProcessor.ReadInt();
                var gX = chunkProcessor.ReadInt();
                var gY = chunkProcessor.ReadInt();
                var bX = chunkProcessor.ReadInt();
                var bY = chunkProcessor.ReadInt();
            }
            else if (type.Equals(PngChunkName.IDAT) == true)
            {
                var block = chunkProcessor.ReadBytes((int)(chunkProcessor.Length));
                this.CompressedScanData.Write(block, 0, block.Length);
            }
            else if (type.Equals(PngChunkName.iCCP) == true)
            {
                var name = chunkProcessor.ReadBytesWhile0();
                var compressionMethod = chunkProcessor.ReadByte();
                var compressionBytes = chunkProcessor.ReadBytes((int)chunkProcessor.Remain);

                using (var iccpSream = new MemoryStream(compressionBytes))
                {
                    using (var zs = new ZlibStream(iccpSream, CompressionMode.Decompress))
                    {
                        var profile = new ICCProfile(zs);
                    }

                }

            }
            else if (type.Equals(PngChunkName.pHYs) == true)
            {
                this.XPixelsPerUnit = chunkProcessor.ReadInt();
                this.YPixelsPerUnit = chunkProcessor.ReadInt();
                this.PhysicalPixelDimensionsUnit = (PngPhysicalPixelDimensionsUnit)chunkProcessor.ReadByte();
            }
            else if (type.Equals(PngChunkName.tRNS) == true)
            {
                if (this.ColorType == PngColorType.Greyscale)
                {
                    chunkProcessor.ReadShort();
                }
                else if (this.ColorType == PngColorType.Truecolor)
                {
                    var r = chunkProcessor.ReadShort();
                    var g = chunkProcessor.ReadShort();
                    var b = chunkProcessor.ReadShort();
                }
                else if (this.ColorType == PngColorType.IndexedColor)
                {
                    var alphaTable = new List<byte>();

                    while (chunkProcessor.Position < chunkProcessor.Length)
                    {
                        var a = chunkProcessor.ReadByte();
                        alphaTable.Add(a);
                    }

                    this.AlphaTable = alphaTable.ToArray();
                }

            }
            else if (type.Equals(PngChunkName.IEND) == true)
            {

            }
            else
            {
                this.ExtraChunks.Add(new PNGRawChunk
                {
                    Name = chunkStream.Name,
                    Payload = chunkProcessor.ReadBytes((byte)chunkProcessor.Length)
                });
            }

        }

        public override ImageArgb32 Decode()
        {
            var width = this.Width;
            var height = this.Height;
            var bitsPerPixel = this.PixelFormat.GetBitsPerPixel();
            var samples = bitsPerPixel / this.BitDepth;
            var colorTable = ColorTableUtils.MergeColorTable(this.RgbTable, this.AlphaTable);
            var scanData = CreateScanData(width, height, bitsPerPixel, this.Stride, this.Interlace, colorTable);

            this.CompressedScanData.Position = 0L;

            using (var ds = new ZlibStream(this.CompressedScanData, CompressionMode.Decompress, true))
            {
                var passProcessor = new InterlacePassProcessor(scanData);
                var dataProcessor = PngCodec.CreatePngProcessor(ds);
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

            var scanProcessor = this.CreateScanProcessor();
            var densityUnit = this.PhysicalPixelDimensionsUnit.ToDensityUnit();
            return new ImageArgb32(scanData, scanProcessor)
            {
                WidthResoulution = new PhysicalDensity(this.XPixelsPerUnit, densityUnit),
                HeightResoulution = new PhysicalDensity(this.YPixelsPerUnit, densityUnit),
            };
        }

        public override void Encode(ImageArgb32 image, PngEncodeOptions options)
        {
            this.Width = image.Width;
            this.Height = image.Height;
            this.PixelFormat = image.PixelFormat;
            this.Interlace = options.Interlace ? (byte)1 : (byte)0;

            var physicalUnit = image.Resolution.Unit;
            this.PhysicalPixelDimensionsUnit = physicalUnit.ToPngPhysicalPixelDimensionsUnit();
            this.XPixelsPerUnit = (int)image.WidthResoulution.GetConvertValue(physicalUnit);
            this.YPixelsPerUnit = (int)image.HeightResoulution.GetConvertValue(physicalUnit);

            var colorTable = new Argb32[0];

            if (options.ColorType.HasValue == true)
            {
                colorTable = image.GetColorTable(PngColorTypeExtensions.ToPixelFormat(options.ColorType.Value, options.BitDepth));
            }
            else
            {
                var usedColors = image.Colors.Distinct().ToArray();
                var noAlpha = usedColors.All(c => c.A == 255);

                var format = usedColors.Length.GetPrefferedIndexedFormat();

                if (format == PixelFormat.Undefined)
                {
                    this.ColorType = noAlpha ? PngColorType.Truecolor : PngColorType.TruecolorWithAlpha;
                    this.BitDepth = 8;
                }
                else
                {
                    this.PixelFormat = format;
                    colorTable = usedColors;
                }

            }

            (this.RgbTable, this.AlphaTable) = ColorTableUtils.SplitColorTable(colorTable);

            var bitPerPixel = this.PixelFormat.GetBitsPerPixel();
            var samples = bitPerPixel / this.BitDepth;
            this.CompressedScanData.Position = 0L;

            var scanData = CreateScanData(image.Width, image.Height, this.PixelFormat.GetBitsPerPixel(), this.Stride, this.Interlace, colorTable);
            var scanProcessor = this.CreateScanProcessor();
            scanProcessor.Write(scanData, image.Scan);

            using (var ds = new ZlibStream(this.CompressedScanData, CompressionMode.Compress, options.CompressionLevel.ToZlibCompressionLevel(), true))
            {
                var passProcessor = new InterlacePassProcessor(scanData);
                var dataProcessor = PngCodec.CreatePngProcessor(ds);
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

        }

        public void Write(DataProcessor output)
        {
            this.WriteChunk(output, PngChunkName.IHDR, chunkProcessor =>
            {
                chunkProcessor.WriteInt(this.Width);
                chunkProcessor.WriteInt(this.Height);
                chunkProcessor.WriteByte(this.BitDepth);
                chunkProcessor.WriteByte((byte)this.ColorType);
                chunkProcessor.WriteByte(this.Compression);
                chunkProcessor.WriteByte(this.Filter);
                chunkProcessor.WriteByte(this.Interlace);
            });

            this.WriteChunk(output, PngChunkName.pHYs, chunkProcessor =>
            {
                chunkProcessor.WriteInt(this.XPixelsPerUnit);
                chunkProcessor.WriteInt(this.YPixelsPerUnit);
                chunkProcessor.WriteByte((byte)this.PhysicalPixelDimensionsUnit);
            });

            if (this.ColorType == PngColorType.IndexedColor)
            {
                this.WriteChunk(output, PngChunkName.PLTE, chunkProcessor =>
                {
                    foreach (var color in this.RgbTable)
                    {
                        chunkProcessor.WriteByte(color.R);
                        chunkProcessor.WriteByte(color.G);
                        chunkProcessor.WriteByte(color.B);
                    }
                });

                if (ColorTableUtils.RequireWriteAlphaTable(this.AlphaTable) == true)
                {
                    this.WriteChunk(output, PngChunkName.tRNS, chunkProcessor =>
                    {
                        foreach (var alpha in this.AlphaTable)
                        {
                            chunkProcessor.WriteByte(alpha);
                        }
                    });
                }

            }

            foreach (var chunk in this.ExtraChunks)
            {
                this.WriteChunk(output, chunk.Name, chunkProcessor =>
                {
                    chunkProcessor.WriteBytes(chunk.Payload);
                });
            }

            this.CompressedScanData.Position = 0L;

            var buffer = new byte[32768];

            for (var len = 0; (len = this.CompressedScanData.Read(buffer, 0, buffer.Length)) > 0;)
            {
                this.WriteChunk(output, PngChunkName.IDAT, chunkProcessor =>
                {
                    chunkProcessor.WriteBytes(buffer);
                });
            }

            this.WriteChunk(output, PngChunkName.IEND, chunkProcessor => { });
        }

        private void WriteChunk(DataProcessor output, PngChunkName name, Action<DataProcessor> action)
        {
            using (var ms = new MemoryStream())
            {
                var chunkProcessor = PngCodec.CreatePngProcessor(ms);
                action(chunkProcessor);

                using (var chunkStream = new PngChunkStream(output, name, (int)ms.Length))
                {
                    ms.Position = 0L;
                    ms.CopyTo(chunkStream);
                }

            }

        }

        private ScanProcessor CreateScanProcessor()
        {
            return ScanProcessor.CreateScanProcessor(this.PixelFormat.GetBitsPerPixel(), this.HasAlpha ? 0xFF000000 : 0x00000000, 0x000000FF, 0x0000FF00, 0x00FF0000);
        }

    }

}