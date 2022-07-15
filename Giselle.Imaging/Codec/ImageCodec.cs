using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec
{
    public abstract class ImageCodec
    {
        public abstract bool SupportMultiFrame { get; }

        public virtual string PrimaryExtension => this.GetExtensions().First();

        public abstract IEnumerable<string> GetExtensions();

        public abstract int BytesForTest { get; }

        public bool Test(Stream stream)
        {
            if (stream.CanSeek == false)
            {
                return false;
            }

            var start = stream.Position;

            try
            {
                return this.TestAsStream(stream);
            }
            finally
            {
                stream.Position = start;
            }

        }

        public bool Test(byte[] bytes) => this.Test(bytes, 0, bytes.Length);

        public bool Test(byte[] bytes, int offset, int count)
        {
            if (count < this.BytesForTest)
            {
                return false;
            }
            else
            {
                using (var ms = new MemoryStream(bytes, offset, count))
                {
                    return this.TestAsStream(ms);
                }

            }

        }

        protected virtual bool TestAsStream(Stream stream)
        {
            var bytes = new byte[this.BytesForTest];
            var prev = stream.Position;
            var len = stream.Read(bytes, 0, bytes.Length);
            stream.Position = prev;

            if (len != bytes.Length)
            {
                return false;
            }
            else
            {
                return this.TestAsBytes(bytes, 0, bytes.Length);
            }

        }

        protected virtual bool TestAsBytes(byte[] bytes, int offset, int count)
        {
            return false;
        }

        public abstract ImageArgb32Container Read(Stream input);

        public void Write(Stream output, ImageArgb32Frame frame, SaveOptions options) => this.Write(output, new ImageArgb32Container() { frame }, options);

        public abstract void Write(Stream output, ImageArgb32Container container, SaveOptions options);

        public abstract PixelFormat GetPreferredPixelFormat(ImageArgb32Frame frame);

        public virtual IEnumerable<PixelFormat> GetSupportIndexedPixelFormats()
        {
            yield break;
        }

    }

}
