using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public abstract class ImageCodec
    {
        public ScanData Read(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return this.Read(bytes);
            }

        }

        public abstract ScanData Read(Stream input);

        public abstract bool Test(byte[] bytes);

    }

}
