using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Giselle.Imaging.Codec.Riff
{
    public static class TypeKeyExtensions
    {
        public static string TypeKeyToString(this int typeKey)
        {
            using (var ms = new MemoryStream())
            {
                var processor = RiffChunk.CreateRiffDataProcessor(ms);
                processor.WriteInt(typeKey);
                var bytes = ms.ToArray();
                return Encoding.ASCII.GetString(bytes);
            }

        }

    }

}
