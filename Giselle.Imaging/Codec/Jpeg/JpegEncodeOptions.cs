using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Jpeg
{
    public class JpegEncodeOptions : SaveOptions<JpegEncodeOptions>
    {
        public JpegEncodeOptions()
        {

        }

        public JpegEncodeOptions(JpegEncodeOptions other)
        {

        }

        public override JpegEncodeOptions Clone() => new(this);
    }

}
