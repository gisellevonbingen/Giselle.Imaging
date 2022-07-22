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

    }

}
