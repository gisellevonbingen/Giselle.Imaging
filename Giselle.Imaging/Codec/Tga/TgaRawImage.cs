using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;
using Giselle.Imaging.Scan;
using Giselle.Imaging.Utils;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaRawImage
    {
        public ushort Width { get; set; } = 0;
        public ushort Height { get; set; } = 0;
        public byte PixelDepth { get; set; } = 32;
        public int Stride => ScanProcessor.GetStride(this.Width, this.PixelDepth, 1);
        public TgaPixelFormat TgaPixelFormat
        {
            get => this.ImageType.ToTgaPixelFormat(this.PixelDepth, this.AlphaBits);
            set => (this.ImageType, this.PixelDepth, this.AlphaBits) = value.ToTgaImageType();
        }
        public PixelFormat PixelFormat
        {
            get => this.TgaPixelFormat.ToPixelFormat();
            set => this.TgaPixelFormat = value.ToTgaPixelFormat();
        }

        public TgaImageType ImageType { get; set; } = TgaImageType.TrueColor;
        public bool Compression { get; set; } = true;
        public ushort OriginX { get; set; } = 0;
        public ushort OriginY { get; set; } = 0;
        public byte AlphaBits { get; set; } = 0;
        public bool FlipX { get; set; } = false;
        public bool FlipY { get; set; } = false;
        public byte[] UncompressedScan { get; set; } = new byte[0];
        public Argb32[] ColorTable { get; set; } = new Argb32[0];

        public TgaRawImage()
        {

        }

        public TgaRawImage(Stream input)
        {
            this.Read(input);
        }

        public TgaRawImage(ImageArgb32Frame frame, TgaSaveOptions options)
        {
            this.Encode(frame, options);
        }

        public void Read(Stream input)
        {
            var processor = TgaCodec.CreateTgaProcessor(input);

            var header = new TgaRawHeader(processor);
            this.Width = header.Width;
            this.Height = header.Height;
            this.PixelDepth = header.PixelDepth;
            this.ImageType = header.ImageType;
            this.Compression = header.Compression;
            this.OriginX = header.OriginX;
            this.OriginY = header.OriginY;
            this.AlphaBits = header.AlphaChannelBits;
            this.FlipX = header.FlipX;
            this.FlipY = header.FlipY;

            var stride = this.Stride;
            var id = processor.ReadBytes(header.IDLength);

            if (this.ImageType == TgaImageType.ColorMapped)
            {
                var pixel = new byte[ScanProcessor.GetPaddedQuotient(header.ColorMapEntryBitDepth, 8)];
                var hasAlpha = pixel.Length > 3;
                this.ColorTable = new Argb32[header.ColorMapLength];

                for (var i = 0; i < header.ColorMapLength; i++)
                {
                    processor.ReadBytes(pixel);
                    this.ColorTable[i] = new Argb32(hasAlpha ? pixel[3] : byte.MaxValue, pixel[2], pixel[1], pixel[0]);
                }

            }
            else
            {
                this.ColorTable = new Argb32[0];
            }

            this.UncompressedScan = new byte[stride * this.Height];

            if (this.Compression == true)
            {
                var bytesPerPixel = stride / this.Width;
                var scanOffset = 0;

                while (scanOffset < this.UncompressedScan.Length)
                {
                    var packetHeader = processor.ReadByte();
                    var packetType = (packetHeader & 0x80) == 0x80;
                    var pixelCount = (packetHeader & 0x7F) + 1;

                    if (packetType == true)
                    {
                        var pixel = processor.ReadBytes(bytesPerPixel);

                        for (var i = 0; i < pixelCount; i++)
                        {
                            Array.Copy(pixel, 0, this.UncompressedScan, scanOffset, bytesPerPixel);
                            scanOffset += bytesPerPixel;
                        }

                    }
                    else
                    {
                        var length = processor.Read(this.UncompressedScan, scanOffset, pixelCount * bytesPerPixel);
                        scanOffset += length;
                    }

                }

            }
            else
            {
                processor.Read(this.UncompressedScan, 0, this.UncompressedScan.Length);
            }

            if (processor.TryGetRemain(out var remain) == true)
            {
                if (remain >= TgaFileFooter.Length)
                {
                    using (var siphonBlock = new SiphonBlock(input, (int)processor.ReadLength, (int)(remain - TgaFileFooter.Length)))
                    {
                        var siphonProcessor = TgaCodec.CreateTgaProcessor(siphonBlock.Siphon);
                        var footer = new TgaFileFooter(processor);

                        if (footer.DeveloperAreaOffset > 0)
                        {
                            siphonBlock.SetPosition(footer.DeveloperAreaOffset);
                        }

                        if (footer.ExtensionOffset > 0)
                        {
                            siphonBlock.SetPosition(footer.ExtensionOffset);
                            var extensionArea = new TgaExtensionArea(siphonProcessor);
                        }

                    }

                }

            }

        }

        public CoordTransformer GetCoordTransformer() => new CoordTransformerFlip(this.FlipX, !this.FlipY);

        public virtual ImageArgb32Frame Decode()
        {
            var format = this.PixelFormat;
            var scanProcessor = ScanProcessor.GetScanProcessor(format);

            var width = this.Width;
            var height = this.Height;
            var stride = this.Stride;

            var scanData = new ScanData(width, height, format.GetBitsPerPixel()) { Stride = stride, Scan = this.UncompressedScan, ColorTable = this.ColorTable, CoordTransformer = this.GetCoordTransformer() };
            var image = new ImageArgb32Frame(scanData, scanProcessor)
            {
                PrimaryCodec = TgaCodec.Instance,
                PrimaryOptions = new TgaSaveOptions() { PixelFormat = this.TgaPixelFormat, Compression = this.Compression, OriginX = this.OriginX, OriginY = this.OriginY, FlipX = this.FlipX, FlipY = this.FlipY },
            };
            return image;
        }

        public void Encode(ImageArgb32Frame frame, TgaSaveOptions options)
        {
            this.Width = (ushort)frame.Width;
            this.Height = (ushort)frame.Height;
            this.TgaPixelFormat = options.PixelFormat;
            this.Compression = options.Compression;
            this.OriginX = options.OriginX;
            this.OriginY = options.OriginY;
            this.FlipX = options.FlipX;
            this.FlipY = options.FlipY;

            var stride = this.Stride;
            var format = this.PixelFormat;

            this.UncompressedScan = new byte[stride * this.Height];

            if (this.ImageType == TgaImageType.ColorMapped)
            {
                this.ColorTable = frame.GetColorTable(format);
            }
            else
            {
                this.ColorTable = new Argb32[0];
            }

            var scanData = new ScanData(this.Width, this.Height, format.GetBitsPerPixel()) { Stride = stride, Scan = this.UncompressedScan, ColorTable = this.ColorTable, CoordTransformer = this.GetCoordTransformer() };
            var scanProcessor = ScanProcessor.GetScanProcessor(format);
            scanProcessor.Write(scanData, frame);
        }

        public void Write(Stream output)
        {
            var processor = TgaCodec.CreateTgaProcessor(output);
            new TgaRawHeader()
            {
                Width = this.Width,
                Height = this.Height,
                PixelDepth = this.PixelDepth,
                ImageType = this.ImageType,
                Compression = this.Compression,
                OriginX = this.OriginX,
                OriginY = this.OriginY,

                IDLength = 0,
                ColorMapLength = 0,

                FlipX = this.FlipX,
                FlipY = this.FlipY,
                AlphaChannelBits = this.AlphaBits,
            }.Write(processor);

            processor.WriteBytes(new byte[0]); // ID
            processor.WriteBytes(new byte[0]); // ColorMap

            if (this.Compression == true)
            {
                var bytesPerPixel = this.Stride / this.Width;
                var scanOffset = 0;

                while (true)
                {
                    var data = this.NextPacket(this.UncompressedScan, scanOffset, this.UncompressedScan.Length, bytesPerPixel, out var packetType, out var pixelCount);

                    if (pixelCount == 0)
                    {
                        break;
                    }

                    var packetHeader = (byte)(((packetType ? 1 : 0) << 0x07) | (pixelCount - 1));
                    processor.WriteByte(packetHeader);

                    processor.WriteBytes(data);
                    scanOffset += pixelCount * bytesPerPixel;
                }

            }
            else
            {
                processor.Write(this.UncompressedScan, 0, this.UncompressedScan.Length);
            }

        }

        private byte[] NextPacket(byte[] scan, int start, int end, int bytesPerPixel, out bool packetType, out int pixelCount)
        {
            packetType = false;
            pixelCount = 0;

            if (start >= end)
            {
                return new byte[0];
            }

            var data = new List<byte>();
            var pixel = new byte[bytesPerPixel];
            var prevPixel = new byte[bytesPerPixel];
            var first = true;

            Array.Copy(scan, start, pixel, 0, bytesPerPixel);
            start += bytesPerPixel;
            data.AddRange(pixel);
            pixelCount++;

            while (start < end && pixelCount < 127)
            {
                Array.Copy(pixel, 0, prevPixel, 0, bytesPerPixel);
                Array.Copy(scan, start, pixel, 0, bytesPerPixel);
                start += bytesPerPixel;
                var continuous = pixel.SequenceEqual(prevPixel);

                if (first == true)
                {
                    first = false;
                    packetType = continuous;

                    if (continuous == true)
                    {
                        pixelCount++;
                    }

                }
                else if (packetType != continuous)
                {
                    break;
                }
                else
                {
                    if (packetType == false)
                    {
                        data.AddRange(prevPixel);
                    }

                    pixelCount++;
                }

            }

            return data.ToArray();
        }

    }

}
