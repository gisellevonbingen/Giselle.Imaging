using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.IO
{
    public static class StreamExtensions
    {
        public static bool TrySetPosition(this Stream stream, long position)
        {
            try
            {
                stream.Position = position;
                return true;
            }
            catch
            {
                return false;
            }

        }

        public static bool TryGetPosition(this Stream stream, out long position)
        {
            try
            {
                position = stream.Position;
                return true;
            }
            catch
            {
                position = default;
                return false;
            }

        }

        public static bool TryGetLength(this Stream stream, out long length)
        {
            try
            {
                length = stream.Length;
                return true;
            }
            catch
            {
                length = default;
                return false;
            }

        }

    }

}
