using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.ICC
{
    public class ICCTag
    {
        public int Signature { get; set; }
        public byte[] Data { get; set; }

        public ICCTag()
        {

        }

    }

}
