using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifSaveOptions : SaveOptions<GifSaveOptions>
    {
        public ushort Repetitions { get; set; }

        public GifSaveOptions()
        {
            this.Repetitions = 0;
        }

        public GifSaveOptions(GifSaveOptions other) : base(other)
        {
            this.Repetitions = other.Repetitions;
        }

        public override GifSaveOptions Clone() => new GifSaveOptions(this);
    }

}
