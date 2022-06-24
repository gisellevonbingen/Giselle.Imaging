using System;
using System.Collections.Generic;
using System.Text;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Riff
{
    public class RiffChunkElement : RiffChunk
    {
        public override int TypeKey { get; }

        public byte[] Data { get; set; } = new byte[0];

        public RiffChunkElement(int typeKey)
        {
            this.TypeKey = typeKey;
        }

        public override string ToString() => $"{this.TypeKeyToString}, Length:{this.Data.Length}";

        protected override void ReadData(DataProcessor input)
        {
            this.Data = input.ReadBytes(input.Remain);
        }

        protected override void WriteData(DataProcessor output)
        {
            output.WriteBytes(this.Data);
        }

    }

}
