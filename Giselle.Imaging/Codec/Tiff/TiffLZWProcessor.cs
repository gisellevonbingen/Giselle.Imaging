using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.LZW;

namespace Giselle.Imaging.Codec.Tiff
{
    public class TiffLZWProcessor : AbstractLZWProcessor
    {
        public TiffLZWProcessor() : base(8, 12)
        {

        }

        public override int GetCodeLengthGrowThreashold(bool reading) => (int)Math.Pow(2, this.CodeLength + 1) + (reading ? -1 : +1);
    }

}
