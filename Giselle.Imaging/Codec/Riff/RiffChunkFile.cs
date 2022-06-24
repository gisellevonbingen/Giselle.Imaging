using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Giselle.Imaging.Codec.Riff
{
    public class RiffChunkFile : RiffChunkAbstractList
    {
        public RiffChunkFile()
        {

        }

        public override int TypeKey => KnownRiffTypeKeys.Riff;

    }

}
