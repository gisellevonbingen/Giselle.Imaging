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

        public abstract bool Test(byte[] bytes);

        public abstract ScanData Read(Stream input);

        public abstract void Write(Stream output, ScanData data);

        public abstract ScanData Encode(Image32Argb image);
    }

}
