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
        public List<PNGRawChunk> ExtraChunks { get; set; } = new List<PNGRawChunk>();

        public PngRawImage(DataProcessor input)
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

        public PngRawImage(ImageArgb32Frame frame, PngSaveOptions options)
        {
            this.Width = frame.Width;
            this.Height = frame.Height;
            this.PixelFormat = frame.PixelFormat;
            this.Interlace = options.Interlace;

            var physicalUnit = frame.Resolution.Unit;
            this.PhysicalPixelDimensionsUnit = physicalUnit.ToPngPhysicalPixelDimensionsUnit();
            this.XPixelsPerUnit = (int)frame.WidthResoulution.GetConvertValue(physicalUnit);
            this.YPixelsPerUnit = (int)frame.HeightResoulution.GetConvertValue(physicalUnit);
            this.ICCProfile = frame.ICCProfile;

            var colorTable = new Argb32[0];

            if (options.ColorType.HasValue == true)
            {
                colorTable = frame.GetColorTable(PngColorTypeExtensions.ToPixelFormat(options.ColorType.Value, options.BitDepth));
            }
            else
            {
                var usedColors = frame.Colors.Distinct().ToArray();
                var noAlpha = usedColors.All(c => c.A == byte.MaxValue);

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

                using (var zlibStream = new ZlibStream(chunkStream, CompressionMode.Decompress, true))
                {
                    this.ICCProfile = new ICCProfile(zlibStream);
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

            if (this.ICCProfile != null)
            {
                this.WriteChunk(output, PngChunkName.iCCP, chunkProcessor =>
                {
                    chunkProcessor.WriteBytesWith0(Encoding.ASCII.GetBytes("ICC Profile"));
                    chunkProcessor.WriteByte(0);

                    using (var zlibStream = new ZlibStream(chunkProcessor.BaseStream, CompressionMode.Compress, true))
                    {
                        this.ICCProfile.Write(zlibStream);
                    }

                });

            }

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