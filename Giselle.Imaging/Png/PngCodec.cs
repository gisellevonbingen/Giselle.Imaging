using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Giselle.Imaging.Bmp;
using Giselle.Imaging.IO;
using Ionic.Zlib;
using Microsoft.Win32.SafeHandles;

namespace Giselle.Imaging.Png
{
    public class PngCodec : ImageCodec<PngEncodeOptions>
    {
        public static PngCodec Instance { get; } = new PngCodec();
        public static IList<byte> Signature { get; } = Array.AsReadOnly(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });

        public PngCodec()
        {

        }

        public override int BytesForTest => Signature.Count;

        public override bool Test(byte[] bytes) => bytes.StartsWith(Signature);

        public override ImageArgb32 Read(Stream stream)
        {
            var processor = new DataProcessor(stream) { IsBigEndian = true };
            var signature = Signature;
            var read = processor.ReadBytes(signature.Count);

            if (signature.SequenceEqual(read) == false)
            {
                throw new IOException();
            }

            var image = new PngImage();

            while (true)
            {
                using (var chunkStream = new PngChunkStream(processor))
                {
                    try
                    {
                        this.ReadChunk(chunkStream, image);
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

            image.CompressedScanData.Position = 0;

            var bitsPerPixel = image.PixelFormat.GetBitsPerPixel();
            var samples = bitsPerPixel / image.BitDepth;
            var stride = image.Stride;
            var scan = new byte[image.Height * stride];

            using (var ds = new ZlibStream(image.CompressedScanData, CompressionMode.Decompress))
            {
                var dataProcessor = new DataProcessor(ds) { IsBigEndian = true };
                var scanline = new byte[stride];

                for (var yi = 0; yi < image.Height; yi++)
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

            var scanData = new ScanData(image.Width, image.Height, stride, bitsPerPixel, scan, image.ColorTable);
            var scanProcessor = ScanProcessor.CreateScanProcessor(bitsPerPixel, image.HasAlpha ? 0xFF000000 : 0x00000000, 0x000000FF, 0x0000FF00, 0x00FF0000);
            return new ImageArgb32(scanData, scanProcessor);
        }

        private void ReadChunk(PngChunkStream chunkStream, PngImage image)
        {
            var chunkProcessor = new DataProcessor(chunkStream) { IsBigEndian = true };
            var type = chunkStream.Type;

            if (type.Equals(PngKnownChunkNames.IHDR) == true)
            {
                image.Width = chunkProcessor.ReadInt();
                image.Height = chunkProcessor.ReadInt();
                image.BitDepth = chunkProcessor.ReadByte();
                image.ColorType = (PngColorType)chunkProcessor.ReadByte();
                image.Compression = chunkProcessor.ReadByte();
                image.Filter = chunkProcessor.ReadByte();
                image.Interlace = chunkProcessor.ReadByte();
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

                image.ColorTable = colorTable.ToArray();
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
                image.CompressedScanData.Write(block, 0, block.Length);
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
                var widthpu = chunkProcessor.ReadInt();
                var heightpu = chunkProcessor.ReadInt();
                var unit = chunkProcessor.ReadByte();
            }
            else if (type.Equals(PngKnownChunkNames.tRNS) == true)
            {
                if (image.ColorType == PngColorType.Greyscale)
                {
                    chunkProcessor.ReadShort();
                }
                else if (image.ColorType == PngColorType.Truecolor)
                {
                    var r = chunkProcessor.ReadShort();
                    var g = chunkProcessor.ReadShort();
                    var b = chunkProcessor.ReadShort();
                }
                else if (image.ColorType == PngColorType.IndexedColor)
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
                var raw = new PNGRawChunk(chunkStream);
            }

        }

        public override void Write(Stream output, ImageArgb32 image, PngEncodeOptions options)
        {
            throw new NotImplementedException();
        }

    }

}
