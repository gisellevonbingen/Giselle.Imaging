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
        bool Test(byte[] bytes);

        ScanData Read(Stream input);

        void Write(Stream output, ScanData data);

        void Write(Stream output, Image32Argb image);

        void Write(Stream output, Image32Argb image, EncodeOptions options);

        ScanData Encode(Image32Argb image);

        ScanData Encode(Image32Argb image, EncodeOptions options);
    }

    public interface IImageCodec<in T> : IImageCodec where T : EncodeOptions, new()
    {
        void Write(Stream output, Image32Argb image, T options);

        ScanData Encode(Image32Argb image, T options);
    }

    public abstract class ImageCodec<T> : IImageCodec<T> where T : EncodeOptions, new()
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

        public void Write(Stream output, Image32Argb image)
        {
            var scanData = this.Encode(image);
            this.Write(output, scanData);
        }

        public void Write(Stream output, Image32Argb image, T option)
        {
            var scanData = this.Encode(image);
            this.Write(output, scanData);
        }


        public abstract void Write(Stream output, ScanData data);

        public abstract ScanData Encode(Image32Argb image, T option);

        public ScanData Encode(Image32Argb image) => this.Encode(image, new T());

        void IImageCodec.Write(Stream output, Image32Argb image, EncodeOptions options) => this.Write(output, image, (options as T) ?? new T());

        ScanData IImageCodec.Encode(Image32Argb image, EncodeOptions option) => this.Encode(image, (option as T) ?? new T());
    }

}
