using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;

namespace Giselle.Imaging.Codec
{
    public static class CommonCompressionLevelExtensions
    {
        public static CompressionLevel ToZlibCompressionLevel(this CommonCompressionLevel value)
        {
            if (value == CommonCompressionLevel.None) return CompressionLevel.None;
            if (value == CommonCompressionLevel.Default) return CompressionLevel.Default;
            else if (value == CommonCompressionLevel.BestSpeed) return CompressionLevel.BestSpeed;
            else if (value == CommonCompressionLevel.BestCompression) return CompressionLevel.BestCompression;
            else throw new ArgumentException($"Unknown CommonCompressionLevel : {value}");
        }

    }

}
