using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.ICC;
using Giselle.Imaging.IO;
using Giselle.Imaging.Physical;
using Giselle.Imaging.Scan;
using Ionic.Zlib;

namespace Giselle.Imaging.Codec.Png
{
    public class PngRawImage
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
        public ICCProfile ICCProfile { get; set; }
        public byte RenderingIntent { get; set; }
        public byte[] ImageGamma { get; set; } = new byte[4];
        public Dictionary<string, string> Texts { get; } = new Dictionary<string, string>();
        public List<PNGRawChunk> ExtraChunks { get; set; } = new List<PNGRawChunk>();

        public PngRawImage()
        {

        }

        public PngRawImage(Stream input) : this()
        {
            this.Read(input);
        }

        public void Read(Stream input)
        {
            var processor = PngCodec.CreatePngProcessor(input);
            var signature = processor.ReadBytes(PngCodec.Instance.BytesForTest);

            if (PngCodec.Instance.Test(signature) == false)
            {
                throw new IOException();
            }

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
                        chunkStream.IgnoreCRC = true;
                        break;
                    }

                }

            }

        }

        public PngRawImage(ImageArgb32Frame frame, PngSaveOptions options) : this()
        {
            this.Encode(frame, options);
        }

        public void Encode(ImageArgb32Frame frame, PngSaveOptions options)
        {
            this.Width = frame.Width;
            this.Height = frame.Height;

            if (options.PixelFormat == PngPixelFormat.Undefined)
            {
                this.PngPixelFormat = PngCodec.Instance.GetPreferredPixelFormat(frame).ToPngPixelFormat();
            }
            else
            {
                this.PngPixelFormat = options.PixelFormat;
            }

            this.Interlace = options.Interlace;

            var physicalUnit = PhysicalUnit.Meter;
            this.PhysicalPixelDimensionsUnit = physicalUnit.ToPngPhysicalPixelDimensionsUnit();
            this.XPixelsPerUnit = (int)frame.WidthResoulution.GetConvertValue(physicalUnit);
            this.YPixelsPerUnit = (int)frame.HeightResoulution.GetConvertValue(physicalUnit);
            this.ICCProfile = frame.ICCProfile;

            var pixelFormat = this.PixelFormat;
            var bitsPerPixel = pixelFormat.GetBitsPerPixel();
            var colorTable = frame.GetColorTable(pixelFormat);
            (this.RgbTable, this.AlphaTable) = ColorTableUtils.SplitColorTable(colorTable);

            var scanData = CreateScanData(frame.Width, frame.Height, bitsPerPixel, this.Stride, this.Interlace, colorTable);
            var scanProcessor = this.CreateScanProcessor();
            scanProcessor.Encode(scanData, frame);

            var bitPerPixel = bitsPerPixel;
            var samples = bitPerPixel / this.BitDepth;
            this.CompressedScanData.Position = 0L;

            using (var ds = new ZlibStream(this.CompressedScanData, CompressionMode.Compress, options.CompressionLevel.ToZlibCompressionLevel(), true))
            {
                var passProcessor = new InterlacePassProcessor(scanData);
                var dataProcessor = PngCodec.CreatePngProcessor(ds);
                var scanOffset = 0;

                while (passProcessor.NextPass() == true)
                {
                    var passInfo = passProcessor.PassInfo;
                    var filteredScanline = new byte[passInfo.Stride];
                    var originalScanline = new byte[passInfo.Stride];

                    for (var yi = 0; yi < passInfo.PixelsY; yi++)
                    {
                        byte filter = 1;
                        var currLineSamples = new byte[samples];
                        var lastLineSamples = new byte[samples];

                        for (var xi = 0; xi < passInfo.Stride; xi++)
                        {
                            var x = scanData.Scan[scanOffset++];
                            var a = currLineSamples[xi % samples];
                            var b = originalScanline[xi];
                            var c = lastLineSamples[xi % samples];
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
                            else if (filter == 4)
                            {
                                result = (byte)(x - ComputePaeth(a, b, c));
                            }
                            else
                            {
                                throw new NotImplementedException($"Filter is {filter}");
                            }

                            filteredScanline[xi] = result;
                            originalScanline[xi] = x;
                            currLineSamples[xi % samples] = x;
                            lastLineSamples[xi % samples] = b;
                        }

                        dataProcessor.WriteByte(filter);
                        dataProcessor.WriteBytes(filteredScanline);
                    }

                }

            }

        }

        public PixelFormat PixelFormat
        {
            get => this.PngPixelFormat.ToPixelFormat();
            set => this.PngPixelFormat = value.ToPngPixelFormat();
        }

        public PngPixelFormat PngPixelFormat
        {
            get => PngPixelFormatExtensions.ToPngPixelFormat(this.ColorType, this.BitDepth);
            set => (this.ColorType, this.BitDepth) = value.ToPngColorType();
        }

        public static int GetStride(int width, PngPixelFormat pngPixelFormat)
        {
            var bitsPerPixel = pngPixelFormat.ToPixelFormat().GetBitsPerPixel();
            return ScanProcessor.GetBytesPerWidth(width, bitsPerPixel);
        }

        public int Stride => GetStride(this.Width, this.PngPixelFormat);

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
                var rgb = new byte[3];

                while (chunkProcessor.Remain > 0)
                {
                    chunkProcessor.ReadBytes(rgb);
                    var color = new Argb32(rgb[0], rgb[1], rgb[2]);
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
                var block = chunkProcessor.ReadBytes(chunkProcessor.Remain);
                this.CompressedScanData.Write(block, 0, block.Length);
            }
            else if (type.Equals(PngChunkName.iCCP) == true)
            {
                var name = chunkProcessor.ReadStringUntil0(Encoding.ASCII);
                var compressionMethod = chunkProcessor.ReadByte();

                using (var zlibStream = new ZlibStream(chunkStream, CompressionMode.Decompress, true))
                {
                    this.ICCProfile = new ICCProfile(zlibStream);
                }

            }
            else if (type.Equals(PngChunkName.sRGB) == true)
            {
                this.RenderingIntent = chunkProcessor.ReadByte();
            }
            else if (type.Equals(PngChunkName.gAMA) == true)
            {
                this.ImageGamma = chunkProcessor.ReadBytes(4);
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
                    var rgb = chunkProcessor.ReadBytes(3);
                }
                else if (this.ColorType == PngColorType.IndexedColor)
                {
                    this.AlphaTable = chunkProcessor.ReadBytes(chunkProcessor.Remain);
                }

            }
            else if (type.Equals(PngChunkName.tEXt) == true)
            {
                var keyword = chunkProcessor.ReadStringUntil0(Encoding.ASCII);
                var text = chunkProcessor.ReadString(Encoding.ASCII, chunkProcessor.Remain);
                this.Texts[keyword] = text;
            }
            else if (type.Equals(PngChunkName.IEND) == true)
            {

            }
            else
            {
                this.ExtraChunks.Add(new PNGRawChunk
                {
                    Name = chunkStream.Name,
                    Payload = chunkProcessor.ReadBytes(chunkProcessor.Remain)
                });
            }

        }

        public ImageArgb32Frame Decode()
        {
            var width = this.Width;
            var height = this.Height;
            var bitsPerPixel = this.PixelFormat.GetBitsPerPixel();
            var samples = bitsPerPixel / this.BitDepth;
            var colorTable = ColorTableUtils.MergeColorTable(this.RgbTable, this.AlphaTable);
            var scanData = PngRawImage.CreateScanData(width, height, bitsPerPixel, this.Stride, this.Interlace, colorTable);

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
                                result = (byte)(x + ComputePaeth(a, b, c));
                            }

                            scanline[xi] = result;
                            currLineSamples1[xi % samples] = result;
                            lastLineSamples2[xi % samples] = b;

                            scanData.Scan[scanOffset++] = result;
                        }

                    }

                }

            }

            var scanProcessor = this.CreateScanProcessor();
            var densityUnit = this.PhysicalPixelDimensionsUnit.ToDensityUnit();
            return new ImageArgb32Frame(scanData, scanProcessor)
            {
                PrimaryCodec = PngCodec.Instance,
                PrimaryOptions = new PngSaveOptions() { Interlace = this.Interlace, PixelFormat = this.PngPixelFormat, },
                WidthResoulution = new PhysicalDensity(this.XPixelsPerUnit, densityUnit),
                HeightResoulution = new PhysicalDensity(this.YPixelsPerUnit, densityUnit),
                ICCProfile = this.ICCProfile,
            };
        }

        public static byte ComputePaeth(byte a, byte b, byte c)
        {
            var p = a + b - c;
            var pa = Math.Abs(p - a);
            var pb = Math.Abs(p - b);
            var pc = Math.Abs(p - c);

            if (pa <= pb && pa <= pc)
            {
                return a;
            }
            else
            {
                return pb <= pc ? b : c;
            }

        }

        public void Write(Stream output)
        {
            var processor = PngCodec.CreatePngProcessor(output);
            processor.WriteBytes(PngCodec.Signature);

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

            if (this.ICCProfile != null)
            {
                this.WriteChunk(output, PngChunkName.iCCP, chunkProcessor =>
                {
                    chunkProcessor.WriteStringWith0(Encoding.ASCII, "ICC Profile");
                    chunkProcessor.WriteByte(0);

                    using (var zlibStream = new ZlibStream(chunkProcessor.BaseStream, CompressionMode.Compress, true))
                    {
                        this.ICCProfile.Write(zlibStream);
                    }

                });

            }

            this.WriteChunk(output, PngChunkName.sRGB, chunkProcessor =>
            {
                chunkProcessor.WriteByte(this.RenderingIntent);
            });

            this.WriteChunk(output, PngChunkName.gAMA, chunkProcessor =>
            {
                chunkProcessor.WriteBytes(this.ImageGamma);
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
                    var rgb = new byte[3];

                    foreach (var color in this.RgbTable)
                    {
                        rgb[0] = color.R;
                        rgb[1] = color.G;
                        rgb[2] = color.B;
                        chunkProcessor.WriteBytes(rgb);
                    }
                });

                if (ColorTableUtils.RequireWriteAlphaTable(this.AlphaTable) == true)
                {
                    this.WriteChunk(output, PngChunkName.tRNS, chunkProcessor =>
                    {
                        chunkProcessor.WriteBytes(this.AlphaTable);
                    });
                }

            }

            foreach (var pair in this.Texts)
            {
                this.WriteChunk(output, PngChunkName.tEXt, chunkProcessor =>
                {
                    chunkProcessor.WriteStringWith0(Encoding.ASCII, pair.Key);
                    chunkProcessor.WriteString(Encoding.ASCII, pair.Value);
                });
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
                    chunkProcessor.Write(buffer, 0, len);
                });
            }

            this.WriteChunk(output, PngChunkName.IEND, chunkProcessor => { });
        }

        private void WriteChunk(Stream output, PngChunkName name, Action<DataProcessor> action)
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

        public ScanProcessor CreateScanProcessor()
        {
            return ScanProcessor.CreateScanProcessor(this.PixelFormat.GetBitsPerPixel(), this.HasAlpha ? 0xFF000000 : 0x00000000, 0x000000FF, 0x0000FF00, 0x00FF0000);
        }

    }

}