using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Utils;

namespace Giselle.Imaging.Codec.Png
{
    public static class PngChunkNameExtensions
    {
        public static string ToDisplayString(this PngChunkName value) => BitConverter2.ToASCIIString((int)value, PngCodec.IsLittleEndian);

    }

}
