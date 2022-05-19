using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Checksum;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Png
{
    public class PNGRawChunk
    {
        public PngChunkName Name { get; set; }
        public string DisplayName => this.Name.ToDisplayString();

        private byte[] _Payload = new byte[0];
        public byte[] Payload
        {
            get => this._Payload;
            set => this._Payload = value ?? new byte[0];
        }

        public PNGRawChunk()
        {

        }

    }

}
