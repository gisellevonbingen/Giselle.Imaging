using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.BMP
{
    public class BMPCodec : ImageCodec
    {
        public const int SignatureLength = 2;
        public static IList<byte> SignatureBM { get; } = Array.AsReadOnly(new byte[] { 0x42, 0x4D });
        public static IList<byte> SignatureBA { get; } = Array.AsReadOnly(new byte[] { 0x42, 0x41 });
        public static IList<byte> SignatureCI { get; } = Array.AsReadOnly(new byte[] { 0x43, 0x49 });
        public static IList<byte> SignatureCP { get; } = Array.AsReadOnly(new byte[] { 0x43, 0x50 });
        public static IList<byte> SignatureIC { get; } = Array.AsReadOnly(new byte[] { 0x49, 0x43 });
        public static IList<byte> SignaturePT { get; } = Array.AsReadOnly(new byte[] { 0x50, 0x54 });
        public static IList<IList<byte>> Signatures { get; } = Array.AsReadOnly(new IList<byte>[] { SignatureBM, SignatureBA, SignatureCI, SignatureCP, SignatureIC, SignaturePT });

        public static DataProcessor CreateBMPProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = true };

        public override bool Test(byte[] bytes) => Signatures.Any(s => bytes.StartsWith(s));

        public override RawImage Read(Stream input)
        {
            var processor = CreateBMPProcessor(input);
            var o1 = processor.ReadLength;

            // Header
            var signature = processor.ReadBytes(SignatureLength);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            var fileSize = processor.ReadInt();
            var reserved = processor.ReadInt();
            var dataOffset = processor.ReadInt();

            // Info Header
            var headerSize = processor.ReadInt();
            var width = processor.ReadInt();
            var height = processor.ReadInt();
            var planes = processor.ReadShort();
            var bitsPerPixel = processor.ReadShort();
            var compressionMethod = processor.ReadInt();
            var compressedImageSize = processor.ReadInt();
            var widthPixelsPerMeter = processor.ReadInt();
            var heightPixelsPerMeter = processor.ReadInt();
            var colorsUsed = processor.ReadInt();
            var importantColors = processor.ReadInt();
            var numOfColors = bitsPerPixel == 1 ? 1 : (int)Math.Pow(2, bitsPerPixel);

            // Color Table
            var colorTable = new List<Color>();
            var useColorTable = bitsPerPixel <= 8;

            if (useColorTable == true)
            {
                ReadColorTable(processor, colorTable, colorsUsed);
            }

            // Skip to dataOffset
            var o2 = processor.ReadLength;
            processor.ReadBytes((int)(dataOffset - (o2 - o1)));

            var divisor = 4;
            var w1 = useColorTable ? 1 : (bitsPerPixel / 8);
            var w2 = useColorTable ? (8 / bitsPerPixel) : 1;

            var readingWidth = (width * w1) / w2;
            var readingMod = readingWidth % divisor;
            var readingStride = readingMod == 0 ? readingWidth : (readingWidth - readingMod + divisor);
            var readingPadding = readingStride - readingWidth;

            var dpp = 4;
            var formatWidth = width * dpp;
            var formatMod = formatWidth % divisor;
            var formatStride = formatMod == 0 ? formatWidth : (formatWidth - formatMod + divisor);
            var stride = formatStride;
            var scan = new byte[height * formatStride];

            if (useColorTable == true)
            {
                var maskBase = 0;

                for (var i = 0; i < bitsPerPixel; i++)
                {
                    maskBase |= 1 << i;
                }

                for (var y = height - 1; y > -1; y--)
                {
                    var offsetBase = y * stride;

                    for (var i = 0; i < readingStride; i++)
                    {
                        var b = processor.ReadByte();

                        for (var bi = 0; bi < w2; bi++)
                        {
                            var x = i * w2 + bi;

                            if (x >= width)
                            {
                                break;
                            }

                            var offset = offsetBase + (i * w2 * dpp) + (bi * dpp);

                            var shift = (8 / w2) * (w2 - 1 - bi);
                            var mask = maskBase << shift;
                            var tableIndex = (b & mask) >> shift;
                            var p = colorTable[tableIndex];
                            scan[offset + 0] = p.B;
                            scan[offset + 1] = p.G;
                            scan[offset + 2] = p.R;
                            scan[offset + 3] = 255;
                        }

                    }

                }

            }
            else
            {
                for (var y = height - 1; y > -1; y--)
                {
                    var offsetBase = y * stride;

                    for (var x = 0; x < width; x++)
                    {
                        var offset = offsetBase + (x * dpp);

                        if (bitsPerPixel == 32)
                        {
                            scan[offset + 0] = processor.ReadByte();
                            scan[offset + 1] = processor.ReadByte();
                            scan[offset + 2] = processor.ReadByte();
                            scan[offset + 3] = processor.ReadByte();
                        }
                        else if (bitsPerPixel == 24)
                        {
                            scan[offset + 0] = processor.ReadByte();
                            scan[offset + 1] = processor.ReadByte();
                            scan[offset + 2] = processor.ReadByte();
                            scan[offset + 3] = 255;
                        }

                    }

                    for (var i = 0; i < readingPadding; i++)
                    {
                        processor.ReadByte();
                    }

                }

            }

            return new RawImage(width, height, stride, scan);
        }

        public void ReadColorTable(DataProcessor processor, List<Color> colorTable, int numOfColors)
        {
            for (var i = 0; i < numOfColors; i++)
            {
                var b = processor.ReadByte();
                var g = processor.ReadByte();
                var r = processor.ReadByte();
                var _ = processor.ReadByte();
                colorTable.Add(Color.FromArgb(r, g, b));
            }

        }

    }

}
