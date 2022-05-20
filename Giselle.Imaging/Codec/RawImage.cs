using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec
{
    public interface IRawImage
    {
        ImageArgb32 Decode();

        void Encode(ImageArgb32 image, EncodeOptions options);
    }

    public abstract class RawImage<T> : IRawImage where T : EncodeOptions, new()
    {
        public abstract ImageArgb32 Decode();

        public abstract void Encode(ImageArgb32 image, T options);

        void IRawImage.Encode(ImageArgb32 image, EncodeOptions options) => this.Encode(image, (options as T) ?? new T());
    }

}
