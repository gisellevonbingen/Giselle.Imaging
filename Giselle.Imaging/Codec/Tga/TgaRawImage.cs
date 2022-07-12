using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;
using Giselle.Imaging.Scan;

namespace Giselle.Imaging.Codec.Tga
{
    public class TgaRawImage
    {
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public byte PixelDepth { get; set; }
        public int Stride => ScanProcessor.GetStride(this.Width, this.PixelDepth, 1);
        public PixelFormat PixelFormat
        {
            get
            {
                var depth = this.PixelDepth;
                if (depth == 8) return PixelFormat.Format8bppIndexed;
                else if (depth == 15) return PixelFormat.Format16bppRgb555;
                else if (depth == 16) return PixelFormat.Format16bppRgb565;
                else if (depth == 24) return PixelFormat.Format24bppRgb888;
                else if (depth == 32) return PixelFormat.Format32bppArgb8888;
                else throw new ArgumentOutOfRangeException();
            }

            set
            {
                if (value == PixelFormat.Format8bppIndexed) this.PixelDepth = 8;
                else if (value == PixelFormat.Format16bppRgb555) this.PixelDepth = 15;
                else if (value == PixelFormat.Format16bppRgb565) this.PixelDepth = 16;
                else if (value == PixelFormat.Format24bppRgb888) this.PixelDepth = 24;
                else if (value == PixelFormat.Format32bppArgb8888) this.PixelDepth = 32;
                else throw new ArgumentOutOfRangeException();
            }

        }

        public byte[] UncompressedScan { get; set; }
        public TgaImageType ImageType { get; set; }

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
            var header = new TgaRawHeader(input);
            this.Width = header.Width;
            this.Height = header.Height;
            this.PixelDepth = header.PixelDepth;
            this.ImageType = header.ImageType;
            var stride = this.Stride;

            var processor = TgaCodec.CreateTgaProcessor(input);
            var id = processor.ReadBytes(header.IDLength);
            var colorMap = processor.ReadBytes(header.ColorMapLength * header.ColorMapEntrySize);

            this.UncompressedScan = new byte[stride * this.Height];

            if (this.ImageType == TgaImageType.NoImage)
            {

            }
            else if (this.ImageType.HasFlag(TgaImageType.RunLengthEncodedFlag) == true)
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

        }

        public virtual ImageArgb32Frame Decode()
        {
            var format = this.PixelFormat;
            var scanProcessor = ScanProcessor.GetScanProcessor(format);

            var width = this.Width;
            var height = this.Height;
            var stride = this.Stride;
            var flippedScan = new byte[this.UncompressedScan.Length];

            for (var i = 0; i < height; i++)
            {
                Array.Copy(this.UncompressedScan, stride * (height - 1 - i), flippedScan, stride * i, stride);
            }

            var scanData = new ScanData(width, height, format.GetBitsPerPixel()) { Stride = stride, Scan = flippedScan };
            var image = new ImageArgb32Frame(scanData, scanProcessor)
            {
                PrimaryCodec = TgaCodec.Instance,
                PrimaryOptions = new TgaSaveOptions() { PixelDepth = this.PixelDepth, ImageType = this.ImageType },
            };
            return image;
        }

        public void Encode(ImageArgb32Frame frame, TgaSaveOptions options)
        {
            this.Width = (ushort)frame.Width;
            this.Height = (ushort)frame.Height;
            this.PixelDepth = options.PixelDepth;
            this.ImageType = options.ImageType;

            var stride = this.Stride;

            var flippedScan = new byte[stride * this.Height];
            var format = this.PixelFormat;
            var scanData = new ScanData(this.Width, this.Height, format.GetBitsPerPixel()) { Stride = stride, Scan = flippedScan };
            var scanProcessor = ScanProcessor.GetScanProcessor(format);
            scanProcessor.Write(scanData, frame.Scan);

            this.UncompressedScan = new byte[stride * this.Height];

            for (var i = 0; i < this.Height; i++)
            {
                Array.Copy(flippedScan, stride * (this.Height - 1 - i), this.UncompressedScan, stride * i, stride);
            }

        }

        public void Write(Stream output)
        {
            var stride = this.Stride;
            new TgaRawHeader()
            {
                Width = this.Width,
                Height = this.Height,
                PixelDepth = this.PixelDepth,
                ImageType = this.ImageType,

                IDLength = 0,
                ColorMapLength = 0,

                AlphaChannelBits = (byte)(this.PixelDepth == 32 ? 0x08 : 0x00),
            }.Write(output);

            var processor = TgaCodec.CreateTgaProcessor(output);
            processor.WriteBytes(new byte[0]); // ID
            processor.WriteBytes(new byte[0]); // ColorMap

            if (this.ImageType == TgaImageType.NoImage)
            {

            }
            else if (this.ImageType.HasFlag(TgaImageType.RunLengthEncodedFlag) == true)
            {
                var bytesPerPixel = stride / this.Width;
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
