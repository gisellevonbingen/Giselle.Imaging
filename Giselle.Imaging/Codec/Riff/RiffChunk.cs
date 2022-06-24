using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Riff
{
    public abstract class RiffChunk
    {
        public const bool IsLittleEndian = true;

        public static DataProcessor CreateRiffDataProcessor(Stream stream) => new DataProcessor(stream) { IsLittleEndian = IsLittleEndian };

        public static RiffChunk ReadChunk(Stream input)
        {
            using (var chunkStream = new RiffChunkStream(input))
            {
                var chunkData = chunkStream.Header;
                RiffChunk chunk;

                if (chunkData.TypeKey == KnownRiffTypeKeys.Riff)
                {
                    chunk = new RiffChunkFile();
                }
                else if (chunkData.TypeKey == KnownRiffTypeKeys.List)
                {
                    chunk = new RiffChunkList();
                }
                else
                {
                    chunk = new RiffChunkElement(chunkData.TypeKey);
                }

                var chunkProcessor = CreateRiffDataProcessor(chunkStream);
                chunk.ReadData(chunkProcessor);

                if (chunkData.Length % 2 == 1)
                {
                    input.ReadByte();
                }

                return chunk;
            }

        }

        public static void WriteChunk(Stream output, RiffChunk chunk)
        {
            using (var ms = new MemoryStream())
            {
                var chunkProcessor = CreateRiffDataProcessor(ms);
                chunk.WriteData(chunkProcessor);
                ms.Position = 0L;

                using (var chunkStream = new RiffChunkStream(output, new RiffChunkHeader(chunk.TypeKey, (int)ms.Length)))
                {
                    ms.CopyTo(chunkStream);

                    if (chunkStream.Length % 2 == 1)
                    {
                        output.WriteByte(0x00);
                    }

                }

            }

        }

        public abstract int TypeKey { get; }

        public string TypeKeyToString => this.TypeKey.TypeKeyToString();

        protected abstract void ReadData(DataProcessor input);

        protected abstract void WriteData(DataProcessor output);
    }

}
