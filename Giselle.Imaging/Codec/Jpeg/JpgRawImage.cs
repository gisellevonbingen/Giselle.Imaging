using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Jpeg
{
    public class JpgRawImage : IRawImage
    {
        public JpgRawImage()
        {

        }

        public ImageArgb32 Decode()
        {
            throw new NotImplementedException();
        }

        public void Encode(ImageArgb32 image, EncodeOptions options)
        {
            throw new NotImplementedException();
        }

    }

}
