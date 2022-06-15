using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Collections;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifContainer
    {
        public const int SignatureLength = 2;
        public static IList<byte> SignatureLittleEndian { get; } = Array.AsReadOnly(new byte[SignatureLength] { 0x49, 0x49 });
        public static IList<byte> SignatureBigEndian { get; } = Array.AsReadOnly(new byte[SignatureLength] { 0x4D, 0x4D });
        public static IList<IList<byte>> Signatures { get; } = Array.AsReadOnly(new IList<byte>[] { SignatureLittleEndian, SignatureBigEndian });

        public const short EndianChecker = 0x002A;

        public static IList<byte> GetSignature(bool isLittleEndian) => isLittleEndian ? SignatureLittleEndian : SignatureBigEndian;

        public static DataProcessor CreateExifProcessor(Stream stream) => new DataProcessor(stream) { };

        public static DataProcessor CreateExifProcessor(Stream stream, DataProcessor processor) => new DataProcessor(stream) { IsLittleEndian = processor.IsLittleEndian };


        public List<ExifImageFileDirectory> Directories { get; } = new List<ExifImageFileDirectory>();
        public byte[] ExtraBytes { get; set; } = new byte[0];

        public ExifContainer()
        {

        }

        public ExifContainer(byte[] input)
        {
            this.Read(input);
        }

        public ExifContainer(Stream input)
        {
            this.Read(input);
        }

        public ExifContainer(Stream input, long origin)
        {
            this.Read(input, origin);
        }

        public void Read(byte[] input)
        {
            using (var ms = new MemoryStream(input))
            {
                this.Read(ms);
            }

        }

        public void Read(Stream input)
        {
            if (input.CanSeek == false)
            {
                throw new ArgumentException("Exif input stream required be seekable");
            }

            var origin = input.Position;
            this.Read(input, origin);
        }

        public void Read(Stream input, long origin)
        {
            var processor = CreateExifProcessor(input);
            var signature = processor.ReadBytes(SignatureLength);
            processor.IsLittleEndian = SignatureLittleEndian.StartsWith(signature);
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

                input.Position = ifdOffset + origin;

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
                        input.Position = raw.ValueOrOffset + origin;
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

                    var entry = new ExifEntry() { TagId = raw.TagId, Value = value };
                    directory.Entries.Add(entry);
                    Console.WriteLine(entry);
                }

            }

        }

    }

}
