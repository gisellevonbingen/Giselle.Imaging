using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Png
{
    public static class PngChunkNameExtensions
    {
        public static string ToDisplayString(this PngChunkName value)
        {
            using (var ms = new MemoryStream())
            {
                var processor = PngCodec.CreatePngProcessor(ms);
                processor.WriteInt((int)value);

                return Encoding.ASCII.GetString(ms.ToArray());
            }

        }

    }

}
