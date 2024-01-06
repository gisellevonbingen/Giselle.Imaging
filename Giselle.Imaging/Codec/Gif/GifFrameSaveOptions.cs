using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifFrameSaveOptions : SaveOptions<GifFrameSaveOptions>
    {
        public bool UserInput { get; set; } = false;
        public ushort Delay { get; set; } = 0;
        public bool Interlace { get; set; } = false;

        public GifFrameSaveOptions()
        {

        }

        public GifFrameSaveOptions(GifFrameSaveOptions other) : base(other)
        {
            this.UserInput = other.UserInput;
            this.Delay = other.Delay;
            this.Interlace = other.Interlace;
        }

        public override GifFrameSaveOptions Clone() => new(this);
    }

}
