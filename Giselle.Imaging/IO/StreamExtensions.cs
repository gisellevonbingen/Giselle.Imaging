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
        public static long GetRemain(this Stream stream) => stream.Length - stream.Position;

        public static bool TryGetRemain(this Stream stream, out long remain)
        {
            if (stream.TryGetPosition(out var position) == true && stream.TryGetLength(out var length) == true)
            {
                remain = length - position;
                return true;
            }
            else
            {
                remain = 0L;
                return false;
            }

        }

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
