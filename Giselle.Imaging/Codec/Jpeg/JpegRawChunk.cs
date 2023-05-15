using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Jpeg
{
    public class JpegRawChunk
    {
        public ushort Marker { get; set; } = 0x0000;
        public byte[] Payload { get; set; } = new byte[0];
    }

}
