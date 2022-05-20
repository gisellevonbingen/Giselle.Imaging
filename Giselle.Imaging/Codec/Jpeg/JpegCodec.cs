using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Codec.Tiff;
using Giselle.Imaging.IO;
using Giselle.Imaging.Utils;

namespace Giselle.Imaging.Codec.Jpeg
{
    public class JpegCodec : ImageCodec<JpgRawImage>
    {
        public static JpegCodec Instance { get; } = new JpegCodec();

        public static DataProcessor CreateJpegProcessor(Stream stream) => new DataProcessor(stream) { IsBigEndian = true };

        public static byte[] GetBytes(ushort marker)
        {
            using (var stream = new MemoryStream())
            {
                var processor = CreateJpegProcessor(stream);
                processor.WriteUShort(marker);

                stream.Position = 0L;
                return processor.ReadBytes(stream.Length);
            }

        }

        public override int BytesForTest => GetBytes((ushort)JpegMarker.SOI).Length;

        public override bool Test(byte[] bytes) => bytes.StartsWith(GetBytes((ushort)JpegMarker.SOI));

        public override JpgRawImage Read(Stream input)
        {
            var processor = CreateJpegProcessor(input);
            var signature = processor.ReadBytes(BytesForTest);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            while (true)
            {
                using (var chunkStream = new JpegChunkStream(processor))
                {
                    var chunkProcessor = CreateJpegProcessor(chunkStream);
                    var marker = chunkStream.Marker;
                    Console.WriteLine($"{marker} : {chunkStream.Length}");

                    if (JpegMarker.APP_0 <= marker && marker <= JpegMarker.APP_F)
                    {
                        var identifierBuilder = new List<byte>();

                        while (chunkStream.Position < chunkStream.Length)
                        {
                            identifierBuilder.Add(chunkProcessor.ReadByte());

                            var identifier = Encoding.ASCII.GetString(identifierBuilder.ToArray(), 0, identifierBuilder.Count - 1);

                            if (identifier.Equals("JFIF") == true)
                            {
                                var versionUpper = chunkProcessor.ReadByte();
                                var versionLower = chunkProcessor.ReadByte();
                                var versionToString = $"{versionUpper}.{versionLower}";
                                var densityUnit = (JpegDensityUnit)chunkProcessor.ReadByte();
                                var densityWidth = chunkProcessor.ReadUShort();
                                var densityHeight = chunkProcessor.ReadUShort();
                                var thumbnailWidth = chunkProcessor.ReadByte();
                                var thumbnailHeight = chunkProcessor.ReadByte();
                            }
                            else if (identifier.Equals("Exif\0") == true)
                            {
                                var raw = TiffCodec.Instance.Read(chunkStream);
                            }
                            else
                            {

                            }

                        }

                    }
                    else if (marker == JpegMarker.SOF_0 || marker == JpegMarker.SOF_1 || marker == JpegMarker.SOF_2 || marker == JpegMarker.SOF_3 ||
                        marker == JpegMarker.SOF_5 || marker == JpegMarker.SOF_6 || marker == JpegMarker.SOF_7 ||
                        marker == JpegMarker.SOF_9 || marker == JpegMarker.SOF_A || marker == JpegMarker.SOF_B ||
                        marker == JpegMarker.SOF_D || marker == JpegMarker.SOF_E || marker == JpegMarker.SOF_F)
                    {
                        var bitsPerSample = chunkProcessor.ReadByte();
                        var height = chunkProcessor.ReadUShort();
                        var samplesPerLine = chunkProcessor.ReadUShort();
                        var components = chunkProcessor.ReadByte();

                        for (var i = 0; i < components; i++)
                        {
                            var componentId = chunkProcessor.ReadByte();
                            var samplingFactors = chunkProcessor.ReadByte();
                            var (horizontalsamplingFactor, verticalSamplingFactor) = BitConverter2.SplitNibbles(samplingFactors);
                            var quantizationTableSelector = chunkProcessor.ReadByte();
                        }

                    }
                    else if (marker == JpegMarker.SOS)
                    {
                        var components = chunkProcessor.ReadByte();

                        for (var i = 0; i < components; i++)
                        {
                            var componentSelector = chunkProcessor.ReadByte();
                            var codingTableSelector = chunkProcessor.ReadByte();
                            var (dcCodingTableSelector, acCodingTableSelector) = BitConverter2.SplitNibbles(codingTableSelector);
                        }

                        var startOfSpectral = chunkProcessor.ReadByte();
                        var endOfSpectral = chunkProcessor.ReadByte();
                        var successiveApproximationBitPositions = chunkProcessor.ReadByte();
                        var (sabUpper, saLower) = BitConverter2.SplitNibbles(successiveApproximationBitPositions);
                    }
                    else if (marker == JpegMarker.DQT)
                    {
                        var b = chunkProcessor.ReadByte();
                        var (elementType, tableId) = BitConverter2.SplitNibbles(b);

                        if (elementType == 0)
                        {
                            var elements = chunkProcessor.ReadArray(64, p => p.ReadByte());
                        }
                        else
                        {
                            var elements = chunkProcessor.ReadArray(64, p => p.ReadUShort());
                        }

                    }
                    else if (marker == JpegMarker.DHT)
                    {
                        var b1 = chunkProcessor.ReadByte();
                        var (tableClass, tableId) = BitConverter2.SplitNibbles(b1);
                        var codeLengths = chunkProcessor.ReadByte();
                        var unknownYet = chunkProcessor.ReadBytes(chunkProcessor.Remain);
                    }
                    else
                    {
                        chunkProcessor.ReadBytes(chunkProcessor.Remain);
                    }

                }

            }

            throw new NotImplementedException();
        }

        public override void Write(Stream output, JpgRawImage image)
        {
            throw new NotImplementedException();
        }

    }

}
