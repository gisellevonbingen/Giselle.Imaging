using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Checksum;

namespace Giselle.Imaging.Codec.Png
{
    public class PNGRawChunk
    {
        public PngChunkName Name { get; set; }
        public string DisplayName => this.Name.ToDisplayString();

        private byte[] _Payload = Array.Empty<byte>();
        public byte[] Payload
        {
            get => this._Payload;
            set => this._Payload = value ?? Array.Empty<byte>();
        }

        public PNGRawChunk()
        {

        }

    }

}
