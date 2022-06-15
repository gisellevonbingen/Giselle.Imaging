using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Algorithms.Huffman;
using Giselle.Imaging.Codec.Exif;
using Giselle.Imaging.Codec.ICC;
using Giselle.Imaging.Codec.Tiff;
using Giselle.Imaging.Collections;
using Giselle.Imaging.IO;
using Giselle.Imaging.Utils;

namespace Giselle.Imaging.Codec.Jpeg
{
    public class JpegCodec : ImageCodec
    {
        public const bool IsLittleEndian = false;
        public const int mcuWidth = 8;
        public const int mcuHeight = 8;
        public static JpegCodec Instance { get; } = new JpegCodec();

        public static DataProcessor CreateJpegProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = IsLittleEndian };

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

        public static int[][] ZigZagOrder = new int[mcuWidth][]{
            new int[mcuHeight]{  0,  1,  5,  6, 14, 15, 27, 28 },
            new int[mcuHeight]{  2,  4,  7, 13, 16, 26, 29, 42 },
            new int[mcuHeight]{  3,  8, 12, 17, 25, 30, 41, 43 },
            new int[mcuHeight]{  9, 11, 18, 24, 31, 40, 44, 53 },
            new int[mcuHeight]{ 10, 19, 23, 32, 39, 45, 52, 54 },
            new int[mcuHeight]{ 20, 22, 33, 38, 46, 51, 55, 60 },
            new int[mcuHeight]{ 21, 34, 37, 47, 50, 56, 59, 61 },
            new int[mcuHeight]{ 35, 36, 48, 49, 57, 58, 62, 63 },
        };

        public override ImageArgb32Container Read(Stream input)
        {
            var processor = CreateJpegProcessor(input);
            var signature = processor.ReadBytes(BytesForTest);

            if (this.Test(signature) == false)
            {
                throw new IOException();
            }

            ushort width = 0;
            ushort height = 0;
            var quantizationTables = new ushort[4][];
            var huffmanDCTables = new Dictionary<byte, HuffmanCode>[4];
            var huffmanACTables = new Dictionary<byte, HuffmanCode>[4];
            var rows = new int[4];
            var cols = new int[4];
            var maxRows = 0;
            var maxCols = 0;
            var currentQuantizationTableSelector = 0;

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

                            var identifier = Encoding.ASCII.GetString(identifierBuilder.ToArray(), 0, identifierBuilder.Count);

                            if (identifier.Equals("JFIF\0") == true)
                            {
                                var versionUpper = chunkProcessor.ReadByte();
                                var versionLower = chunkProcessor.ReadByte();
                                var versionToString = $"{versionUpper}.{versionLower}";
                                var densityUnit = (JpegDensityUnit)chunkProcessor.ReadByte();
                                var densityWidth = chunkProcessor.ReadUShort();
                                var densityHeight = chunkProcessor.ReadUShort();
                                var thumbnailWidth = chunkProcessor.ReadByte();
                                var thumbnailHeight = chunkProcessor.ReadByte();
                                break;
                            }
                            else if (identifier.Equals("Exif\0\0") == true)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    chunkStream.CopyTo(ms);
                                    ms.Position = 0L;
                                    var exif = new ExifContainer();
                                    exif.Read(ms);
                                }

                                break;
                            }
                            else if (identifier.Equals("ICC_PROFILE\0") == true)
                            {
                                chunkProcessor.ReadShort();
                                var profile = new ICCProfile(chunkStream);
                                break;
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
                        height = chunkProcessor.ReadUShort();
                        width = chunkProcessor.ReadUShort();
                        var components = chunkProcessor.ReadByte();

                        for (var i = 0; i < components; i++)
                        {
                            var componentId = chunkProcessor.ReadByte();
                            var samplingFactors = chunkProcessor.ReadByte();
                            var (horizontalsamplingFactor, verticalSamplingFactor) = BitConverter2.SplitNibbles(samplingFactors);
                            var quantizationTableSelector = chunkProcessor.ReadByte();

                            rows[componentId - 1] = horizontalsamplingFactor;
                            cols[componentId - 1] = verticalSamplingFactor;
                            maxRows = Math.Max(maxRows, horizontalsamplingFactor);
                            maxCols = Math.Max(maxCols, verticalSamplingFactor);
                            currentQuantizationTableSelector = quantizationTableSelector;
                            Console.WriteLine($"{componentId:X2}, {horizontalsamplingFactor}, {verticalSamplingFactor}, {quantizationTableSelector}");
                        }

                    }
                    else if (marker == JpegMarker.SOS)
                    {
                        int widthFactor = maxCols * mcuWidth;
                        int heightFactor = maxRows * mcuHeight;
                        int mcuInWidth = (width + widthFactor - 1) / widthFactor;
                        int mcuInHeight = (height + heightFactor - 1) / heightFactor;

                        var components = chunkProcessor.ReadByte();
                        var dcSelectors = new byte[components];
                        var acSelectors = new byte[components];

                        for (var i = 0; i < components; i++)
                        {
                            var componentId = chunkProcessor.ReadByte();
                            var huffmanTableSelector = chunkProcessor.ReadByte();
                            var (dcSelector, acSelector) = BitConverter2.SplitNibbles(huffmanTableSelector);
                            dcSelectors[componentId - 1] = dcSelector;
                            acSelectors[componentId - 1] = acSelector;
                            Console.WriteLine($"{componentId:X2}, {dcSelector}, {acSelector}");
                        }

                        var startOfSpectral = chunkProcessor.ReadByte();
                        var endOfSpectral = chunkProcessor.ReadByte();
                        var successiveApproximationBitPositions = chunkProcessor.ReadByte();
                        var (sabUpper, saLower) = BitConverter2.SplitNibbles(successiveApproximationBitPositions);

                        // Decode Scan

                        using (var entropyStream = new JpegEntropyStream(input, true))
                        {
                            var quantizationTable = quantizationTables[currentQuantizationTableSelector];
                            var prev = new int[components];

                            // Decode MCU

                            for (var my = 0; my < mcuInHeight; my++)
                            {
                                for (var mx = 0; mx < mcuInWidth; mx++)
                                {
                                    // Decode MCU Component

                                    for (var c = 0; c < components; c++)
                                    {
                                        var dcTable = huffmanDCTables[dcSelectors[c]];
                                        var acTable = huffmanACTables[acSelectors[c]];

                                        for (var row = 0; row < rows[c]; row++)
                                        {
                                            for (var col = 0; col < cols[c]; col++)
                                            {
                                                // Read Entropy
                                                var entropy = new int[mcuWidth * mcuHeight];

                                                entropyStream.SetCodeTable(dcTable);
                                                entropy[0] = entropyStream.ReadDC() + prev[c];

                                                entropyStream.SetCodeTable(acTable);
                                                entropyStream.ReadACTable(entropy, 1, entropy.Length - 1);

                                                prev[c] = entropy[0];

                                                // Dequantize
                                                var quantized = new int[entropy.Length];

                                                for (var i = 0; i < entropy.Length; i++)
                                                {
                                                    quantized[i] = entropy[i] * quantizationTable[i];
                                                }

                                                // Reorder ZigZag
                                                var ordereds = new int[quantized.Length];

                                                for (var y = 0; y < mcuHeight; y++)
                                                {
                                                    for (var x = 0; x < mcuWidth; x++)
                                                    {
                                                        var index = y * mcuWidth + x;
                                                        var zig_zag = ZigZagOrder[x][y];
                                                        ordereds[index] = quantized[zig_zag] * quantizationTable[zig_zag];
                                                    }

                                                }

                                            }

                                        }

                                    }

                                }

                            }

                        }

                    }
                    else if (marker == JpegMarker.DQT)
                    {
                        var b = chunkProcessor.ReadByte();
                        var (elementType, tableId) = BitConverter2.SplitNibbles(b);
                        var table = new ushort[mcuWidth * mcuHeight];

                        if (elementType == 0)
                        {
                            chunkProcessor.ReadArray(table, 0, table.Length, p => p.ReadByte());
                        }
                        else
                        {
                            chunkProcessor.ReadArray(table, 0, table.Length, p => p.ReadUShort());
                        }

                        quantizationTables[tableId] = table;
                    }
                    else if (marker == JpegMarker.DHT)
                    {
                        var b1 = chunkProcessor.ReadByte();
                        // Class
                        // 0 : DC
                        // 1 : AC
                        var (tableClass, tableId) = BitConverter2.SplitNibbles(b1);
                        var tableLengths = chunkProcessor.ReadBytes(16);
                        var simbolTable = new byte[tableLengths.Length][];

                        // depth + 1 : length of Huffman Code
                        for (var depth = 0; depth < tableLengths.Length; depth++)
                        {
                            simbolTable[depth] = chunkProcessor.ReadBytes(tableLengths[depth]);
                        }

                        var rootNode = HuffmanNode<byte>.FromSimbolTable(simbolTable);
                        var codeTable = rootNode.ToCodeTable();
                        (tableClass == 0 ? huffmanDCTables : huffmanACTables)[tableId] = codeTable;
                    }
                    else if (marker == JpegMarker.EOI)
                    {
                        break;
                    }
                    else
                    {
                        chunkProcessor.ReadBytes(chunkProcessor.Remain);
                    }

                }

            }

            throw new NotImplementedException();
        }

        public override void Write(Stream output, ImageArgb32Container container, SaveOptions options)
        {
            throw new NotImplementedException();
        }

    }

}
