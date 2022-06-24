using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Ani
{
    public class AniStep
    {
        public int Frame { get; set; }
        public int JIFRate { get; set; }

        public AniStep()
        {

        }

        public override string ToString()
        {
            return $"Frame:{this.Frame}, JIF:{this.JIFRate}";
        }

    }

}
