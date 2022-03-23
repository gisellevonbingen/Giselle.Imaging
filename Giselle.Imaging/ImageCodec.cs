using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging
{
    public interface IImageCodec
    {
        RawImage Read(Stream input);

        bool Test(byte[] bytes);

    }

    public abstract class ImageCodec : IImageCodec
    {
        public RawImage Read(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                return this.Read(bytes);
            }

        }

        public abstract RawImage Read(Stream input);

        public abstract bool Test(byte[] bytes);

    }

}
