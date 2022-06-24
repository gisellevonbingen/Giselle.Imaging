using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Giselle.Imaging.Codec.Riff
{
    public class RiffChunkHeader
    {
        public int TypeKey { get;  }
        public int Length { get; }

        public RiffChunkHeader(int typeKey, int length)
        {
            this.TypeKey = typeKey;
            this.Length = length;
        }

        public string TypeKeyToString => this.TypeKey.TypeKeyToString();

        public override string ToString() => this.TypeKeyToString;

    }

}
