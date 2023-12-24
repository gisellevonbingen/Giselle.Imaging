using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifFrameSaveOptions : SaveOptions<GifFrameSaveOptions>
    {
        public int Delay { get; set; }

        public GifFrameSaveOptions()
        {

        }

        public GifFrameSaveOptions(GifFrameSaveOptions other) : base(other)
        {
            this.Delay = other.Delay;
        }

        public override GifFrameSaveOptions Clone() => new GifFrameSaveOptions(this);
    }

}
