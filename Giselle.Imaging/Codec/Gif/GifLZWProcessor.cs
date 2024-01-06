using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Streams.LZW;

namespace Giselle.Imaging.Codec.Gif
{
    public class GifLZWProcessor : AbstractLZWProcessor
    {
        public GifLZWProcessor(int minimumCodeLength, int maximumCodeLength) : base(minimumCodeLength, maximumCodeLength)
        {

        }

        public override int GetCodeLengthGrowThreashold(bool reading) => (int)Math.Pow(2, this.CodeLength + 1) + (reading ? 0 : 2);
    }

}
