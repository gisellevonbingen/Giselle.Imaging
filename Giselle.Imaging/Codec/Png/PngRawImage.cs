﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;
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
        public Argb32[] ColorTable { get; set; } = new Argb32[0];
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

        public int Stride
        {
            get
            {
                var width = this.Width;
                var colorType = this.ColorType;
                var bitDepth = this.BitDepth;
                var bitsPerPixel = PngColorTypeExtensions.ToPixelFormat(colorType, bitDepth).GetBitsPerPixel();

                if (colorType == PngColorType.IndexedColor)
                {
                    var padding = (bitDepth % 8 == 0) ? 1 : 2;
                    return ScanProcessor.GetStride(width, bitsPerPixel, padding);
                }
                else if (colorType == PngColorType.Truecolor || colorType == PngColorType.TruecolorWithAlpha)
                {
                    return ScanProcessor.GetBytesPerWidth(width, bitsPerPixel);
                }
                else
                {
                    throw new NotImplementedException($"ColorType({colorType}) is Not Supported");
                }

            }

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

                    if (chunkStream.Type.Equals(PngKnownChunkNames.IEND) == true)
                    {
                        break;
                    }

                }

            }

        }

        public ImageArgb32 Decode()
        {
            var width = this.Width;
            var height = this.Height;
            var bitsPerPixel = this.PixelFormat.GetBitsPerPixel();
            var stride = this.Stride;
            var samples = bitsPerPixel / this.BitDepth;
            var scan = new byte[height * stride];
            this.CompressedScanData.Position = 0L;

            using (var ds = new ZlibStream(this.CompressedScanData, CompressionMode.Decompress, true))
            {
                var dataProcessor = PngCodec.CreatePngProcessor(ds);
                var scanline = new byte[stride];

                for (var yi = 0; yi < height; yi++)
                {
                    var filter = dataProcessor.ReadByte();
                    var strideBytes = dataProcessor.ReadBytes(stride);
                    var currLineSamples1 = new byte[samples];
                    var lastLineSamples2 = new byte[samples];

                    for (var xi = 0; xi < stride; xi++)
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
                    }

                    Array.Copy(scanline, 0, scan, yi * stride, stride);
                }

            }

            var scanData = new ScanData(width, height, stride, bitsPerPixel, scan, this.ColorTable);
            var scanProcessor = this.CreateScanProcessor();
            var densityUnit = this.PhysicalPixelDimensionsUnit.ToDensityUnit();
            return new ImageArgb32(scanData, scanProcessor)
            {
                WidthResoulution = new PhysicalDensity(this.XPixelsPerUnit, densityUnit),
                HeightResoulution = new PhysicalDensity(this.YPixelsPerUnit, densityUnit),
            };
        }

        public void Encode(ImageArgb32 image, PngEncodeOptions options)
        {
            this.Width = image.Width;
            this.Height = image.Height;
            this.PixelFormat = image.PixelFormat;
            var colorTable = new Argb32[0];

            if (options.ColorType.HasValue == true)
            {
                colorTable = image.GetColorTable(PngColorTypeExtensions.ToPixelFormat(options.ColorType.Value, options.BitDepth));
            }
            else
            {
                var usedColors = image.Colors.Distinct().ToArray();
                var useAlpha = usedColors.Any(c => c.A < 255);

                if (useAlpha == true)
                {
                    this.ColorType = PngColorType.TruecolorWithAlpha;
                    this.BitDepth = 8;
                }
                else
                {
                    this.PixelFormat = usedColors.Length.GetPrefferedIndexedFormat(PixelFormat.Format24bppRgb888);
                    if (this.ColorType == PngColorType.IndexedColor) colorTable = usedColors;
                }

            }

            this.ColorTable = colorTable;
            var stride = this.Stride;
            var bitPerPixel = image.PixelFormat.GetBitsPerPixel();
            var samples = bitPerPixel / this.BitDepth;
            this.CompressedScanData.Position = 0L;

            var scan = new byte[image.Height * stride];
            var scanData = new ScanData(image.Width, image.Height, stride, this.PixelFormat.GetBitsPerPixel(), scan, this.ColorTable);
            var scanProcessor = this.CreateScanProcessor();
            scanProcessor.Write(scanData, image.Scan);

            using (var ds = new ZlibStream(this.CompressedScanData, CompressionMode.Compress, options.CompressionLevel.ToZlibCompressionLevel(), true))
            {
                var dataProcessor = PngCodec.CreatePngProcessor(ds);
                var scanline = new byte[stride];

                for (var yi = 0; yi < this.Height; yi++)
                {
                    byte filter = 0;
                    var currLineSamples1 = new byte[samples];
                    var lastLineSamples2 = new byte[samples];

                    for (var xi = 0; xi < stride; xi++)
                    {
                        var x = scan[yi * stride + xi];
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

        public void Write(DataProcessor output)
        {
            this.WriteChunk(output, PngKnownChunkNames.IHDR, chunkcProcessor =>
            {
                chunkcProcessor.WriteInt(this.Width);
                chunkcProcessor.WriteInt(this.Height);
                chunkcProcessor.WriteByte(this.BitDepth);
                chunkcProcessor.WriteByte((byte)this.ColorType);
                chunkcProcessor.WriteByte(this.Compression);
                chunkcProcessor.WriteByte(this.Filter);
                chunkcProcessor.WriteByte(this.Interlace);
            });

            if (this.ColorType == PngColorType.IndexedColor)
            {
                this.WriteChunk(output, PngKnownChunkNames.PLTE, chunkcProcessor =>
                {
                    foreach (var color in this.ColorTable)
                    {
                        chunkcProcessor.WriteByte(color.R);
                        chunkcProcessor.WriteByte(color.G);
                        chunkcProcessor.WriteByte(color.B);
                    }
                });
            }

            foreach (var chunk in this.ExtraChunks)
            {
                this.WriteChunk(output, chunk.Type, chunkcProcessor =>
                {
                    chunkcProcessor.WriteBytes(chunk.Data);
                });
            }

            this.CompressedScanData.Position = 0L;

            var buffer = new byte[32768];

            for (var len = 0; (len = this.CompressedScanData.Read(buffer, 0, buffer.Length)) > 0;)
            {
                this.WriteChunk(output, PngKnownChunkNames.IDAT, chunkcProcessor =>
                {
                    chunkcProcessor.WriteBytes(buffer);
                });
            }

            this.WriteChunk(output, PngKnownChunkNames.IEND, chunkcProcessor => { });
        }

        private void WriteChunk(DataProcessor output, string type, Action<DataProcessor> action)
        {
            using (var ms = new MemoryStream())
            {
                var chunkProcessor = PngCodec.CreatePngProcessor(ms);
                action(chunkProcessor);

                using (var chunkStream = new PngChunkStream(output, type, (int)ms.Length))
                {
                    ms.Position = 0L;
                    ms.CopyTo(chunkStream);
                }

            }

        }

        private void ReadChunk(PngChunkStream chunkStream)
        {
            var chunkProcessor = PngCodec.CreatePngProcessor(chunkStream);
            var type = chunkStream.Type;

            if (type.Equals(PngKnownChunkNames.IHDR) == true)
            {
                this.Width = chunkProcessor.ReadInt();
                this.Height = chunkProcessor.ReadInt();
                this.BitDepth = chunkProcessor.ReadByte();
                this.ColorType = (PngColorType)chunkProcessor.ReadByte();
                this.Compression = chunkProcessor.ReadByte();
                this.Filter = chunkProcessor.ReadByte();
                this.Interlace = chunkProcessor.ReadByte();
            }
            else if (type.Equals(PngKnownChunkNames.PLTE) == true)
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

                this.ColorTable = colorTable.ToArray();
            }
            else if (type.Equals(PngKnownChunkNames.gAMA) == true)
            {
                var gamma = chunkProcessor.ReadInt();
            }
            else if (type.Equals(PngKnownChunkNames.cHRM) == true)
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
            else if (type.Equals(PngKnownChunkNames.IDAT) == true)
            {
                var block = chunkProcessor.ReadBytes((int)(chunkProcessor.Length));
                this.CompressedScanData.Write(block, 0, block.Length);
            }
            else if (type.Equals(PngKnownChunkNames.iCCP) == true)
            {
                var name = string.Empty;

                using (var ms = new MemoryStream())
                {
                    while (true)
                    {
                        var b = chunkProcessor.ReadByte();

                        if (b == 0x00)
                        {
                            break;
                        }
                        else
                        {
                            ms.WriteByte(b);
                        }

                    }

                    name = Encoding.ASCII.GetString(ms.ToArray());
                }

                var compressionMethod = chunkProcessor.ReadByte();
                var compressionProfile = chunkProcessor.ReadBytes((int)chunkProcessor.Remain);
            }
            else if (type.Equals(PngKnownChunkNames.pHYs) == true)
            {
                this.XPixelsPerUnit = chunkProcessor.ReadInt();
                this.YPixelsPerUnit = chunkProcessor.ReadInt();
                this.PhysicalPixelDimensionsUnit = (PngPhysicalPixelDimensionsUnit)chunkProcessor.ReadByte();
            }
            else if (type.Equals(PngKnownChunkNames.tRNS) == true)
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
                    var alphas = new List<byte>();

                    while (chunkProcessor.Position < chunkProcessor.Length)
                    {
                        var a = chunkProcessor.ReadByte();
                        alphas.Add(a);
                    }

                }

            }
            else if (type.Equals(PngKnownChunkNames.IEND) == true)
            {

            }
            else
            {
                var rawChunk = new PNGRawChunk();
                rawChunk.Type = chunkStream.Type;
                rawChunk.Data = chunkProcessor.ReadBytes((byte)chunkProcessor.Length);
                this.ExtraChunks.Add(rawChunk);
            }

        }

        private ScanProcessor CreateScanProcessor()
        {
            return ScanProcessor.CreateScanProcessor(this.PixelFormat.GetBitsPerPixel(), this.HasAlpha ? 0xFF000000 : 0x00000000, 0x000000FF, 0x0000FF00, 0x00FF0000);
        }

    }

}