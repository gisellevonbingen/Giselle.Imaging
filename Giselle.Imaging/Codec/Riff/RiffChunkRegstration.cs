using System;
using System.Collections.Generic;
using System.Text;

namespace Giselle.Imaging.Codec.Riff
{
    public class RiffChunkRegstration
    {
        public int TypeKey { get; }
        public Func<RiffChunk> Generator { get; }

        public RiffChunkRegstration(int key, Func<RiffChunk> generator)
        {
            this.TypeKey = key;
            this.Generator = generator;
        }

    }

}
