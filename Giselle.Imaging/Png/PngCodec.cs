﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Bmp;
using Giselle.Imaging.IO;

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
                    this.ReadChunk(chunkStream, image);

                    if (chunkStream.Equals(PngKnownChunkNames.IEND) == true)
                    {
                        break;
                    }

                }

            }

            throw new NotImplementedException();
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

                Console.WriteLine(image.ColorType);
                Console.WriteLine(image.BitDepth);
                Console.WriteLine(image.PixelFormat);
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
                var code = chunkProcessor.ReadByte();
                var cm = (code & 0x0F) >> 0x00;
                var ci = (code & 0xF0) >> 0x04;
                var bits = chunkProcessor.ReadByte();

                var block = chunkProcessor.ReadBytes((int)(chunkProcessor.Length - 6));

                using (var ms = new MemoryStream(block))
                {
                    using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
                    {
                        var dataProcessor = new DataProcessor(ds) { IsBigEndian = true };
                        var hasAlpha = image.HasAlpha;
                        var stride = ScanProcessor.GetStride(image.Width, image.PixelFormat.GetBitsPerPixel());

                        for (var yi = 0; yi < image.Height; yi++)
                        {
                            var x = dataProcessor.ReadByte();
                            var bytes = dataProcessor.ReadBytes(stride);

                            Console.WriteLine("========== FEED ==========");
                        }

                    }

                }

                var checkValue = chunkProcessor.ReadInt();
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
