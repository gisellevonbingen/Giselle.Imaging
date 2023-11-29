using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Utils;

namespace Giselle.Imaging.Codec.ICC
{
    public class ICCTag
    {
        public int Signature { get; set; }
        public byte[] Data { get; set; }

        public ICCTag()
        {

        }

        public ICCTag(ICCTag other)
        {
            this.Signature = other.Signature;
            this.Data = other.Data.ToArray();
        }

        public ICCTag Clone() => new(this);

        public override string ToString()
        {
            return $"Signature: \"{this.Signature.ToASCIIString(ICCProfile.IsLittleEndian)}\", Length: {this.Data.Length}";
        }

    }

}
