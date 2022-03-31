using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.Checksum;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Codec.Png
{
    public class PNGRawChunk
    {
        public const int TypeLength = 4;

        private byte[] _TypeRaw = new byte[TypeLength];
        public byte[] TypeRaw
        {
            get => this._TypeRaw;
            set
            {
                if (value is null)
                {
                    this._TypeRaw = new byte[TypeLength];
                }
                else if (value.Length == TypeLength)
                {
                    this._TypeRaw = value.ToArray();
                }
                else
                {
                    this._TypeRaw = value.TakeElse(TypeLength).ToArray();
                }

            }

        }

        public string Type
        {
            get => Encoding.ASCII.GetString(this.TypeRaw);
            set => this.TypeRaw = value is null ? null : Encoding.ASCII.GetBytes(value);
        }

        private byte[] _Data = new byte[0];
        public byte[] Data
        {
            get => this._Data;
            set => this._Data = value ?? new byte[0];
        }

        public PNGRawChunk()
        {

        }

    }

}
