using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifContainer
    {
        public const int SignatureLength = 2;
        public static byte[] SignatureLittleEndian { get; } = new byte[SignatureLength] { 0x49, 0x49 };
        public static byte[] SignatureBigEndian { get; } = new byte[SignatureLength] { 0x4D, 0x4D };
        public static IList<byte[]> Signatures { get; } = new List<byte[]>() { SignatureLittleEndian, SignatureBigEndian }.AsReadOnly();

        public const short EndianChecker = 0x002A;
        public const int EndianCheckerSize = 2;

        public static byte[] GetSignature(bool isLittleEndian) => isLittleEndian ? SignatureLittleEndian : SignatureBigEndian;

        public static bool TryGetEndian(byte[] signature, out bool isLittleEndian)
        {
            if (signature.SequenceEqual(SignatureLittleEndian) == true)
            {
                isLittleEndian = true;
                return true;
            }
            else if (signature.SequenceEqual(SignatureBigEndian) == true)
            {
                isLittleEndian = false;
                return true;
            }
            else
            {
                isLittleEndian = default;
                return false;
            }

        }

        public static DataProcessor CreateExifProcessor(Stream stream) => new DataProcessor(stream) { };

        public static DataProcessor CreateExifProcessor(Stream stream, DataProcessor processor) => new DataProcessor(stream) { IsLittleEndian = processor.IsLittleEndian };


        public List<ExifImageFileDirectory> Directories { get; } = new List<ExifImageFileDirectory>();

        public ExifContainer()
        {

        }

        public ExifContainer(Stream input)
        {
            this.Read(input);
        }

        public void Read(Stream input)
        {
            using (var siphonBlock = SiphonBlock.ByRemain(input))
            {
                var siphon = siphonBlock.SiphonSteam;
                var processor = CreateExifProcessor(siphon);
                var signature = processor.ReadBytes(SignatureLength);

                if (TryGetEndian(signature, out var isLittleEndian) == false)
                {
                    throw new IOException($"Signature not found");
                }

                processor.IsLittleEndian = isLittleEndian;
                var endianChecker = processor.ReadShort();

                if (endianChecker != EndianChecker)
                {
                    throw new IOException($"Endian Check Failed : Reading={endianChecker:X4}, Require={EndianChecker:X4}");
                }

                this.Directories.Clear();
                var ifdOffset = processor.ReadInt();

                while (true)
                {
                    if (ifdOffset == 0)
                    {
                        break;
                    }

                    siphon.Position = ifdOffset;

                    var entryCount = processor.ReadShort();
                    var rawEntries = new List<ExifRawEntry>();

                    for (var i = 0; i < entryCount; i++)
                    {
                        var entry = new ExifRawEntry();
                        entry.ReadInfo(processor);
                        rawEntries.Add(entry);
                    }

                    ifdOffset = processor.ReadInt();

                    var directory = new ExifImageFileDirectory();
                    this.Directories.Add(directory);

                    foreach (var raw in rawEntries)
                    {
                        var value = raw.ValueType.ValueGenerator();

                        if (raw.IsOffset == true)
                        {
                            siphon.Position = raw.ValueOrOffset;
                            value.Read(raw, processor);
                        }
                        else
                        {
                            using (var ms = new MemoryStream())
                            {
                                var entryProcessor = CreateExifProcessor(ms, processor);
                                entryProcessor.WriteInt(raw.ValueOrOffset);

                                ms.Position = 0L;
                                value.Read(raw, entryProcessor);
                            }

                        }

                        directory.Add(raw.TagId, value);
                    }

                }

            }

        }

        public long InfoSize
        {
            get
            {
                var size = SignatureLength + EndianCheckerSize + (this.Directories.Count + 1) * 4;
                size += this.Directories.Sum(d => 2 + d.Count * ExifRawEntry.InfoSize);
                return size;
            }

        }

        public long InfoWithValuesSize
        {
            get
            {
                var size = this.InfoSize;
                size += this.Directories.Sum(d => d.OffsetValuesSize);

                return size;
            }

        }

        public void Write(Stream output) => this.Write(output, new ExifSaveOptions());

        public void Write(Stream output, ExifSaveOptions options)
        {
            var processor = CreateExifProcessor(output);
            processor.IsLittleEndian = options.IsLittleEndian;
            processor.WriteBytes(GetSignature(processor.IsLittleEndian));
            processor.WriteShort(EndianChecker);

            var dataAreaOffset = this.InfoSize;
            var dataAreaCursor = dataAreaOffset;

            foreach (var directory in this.Directories)
            {
                processor.WriteInt((int)(processor.WriteLength + 4)); // IFD Offset, Add own size, Entries will be list right behind

                var entryCount = (short)directory.Count;
                processor.WriteShort(entryCount);

                foreach (var pair in directory)
                {
                    var raw = new ExifRawEntry(pair);

                    if (raw.IsOffset == true)
                    {
                        raw.ValueOrOffset = (int)dataAreaCursor;
                        dataAreaCursor += raw.ValuesSize;
                    }
                    else
                    {
                        using (var ms = new MemoryStream())
                        {
                            var entryProcessor = CreateExifProcessor(ms, processor);
                            pair.Value.Write(raw, entryProcessor);
                            entryProcessor.WriteInt(0);
                            ms.Position = 0L;

                            raw.ValueOrOffset = entryProcessor.ReadInt();
                        }

                    }

                    raw.WriteInfo(processor);
                }

            }

            processor.WriteInt(0);

            foreach (var directory in this.Directories)
            {
                var entryCount = (short)directory.Count;

                foreach (var pair in directory)
                {
                    var raw = new ExifRawEntry(pair);

                    if (raw.IsOffset == true)
                    {
                        pair.Value.Write(raw, processor);
                    }

                }

            }

        }

    }

}
