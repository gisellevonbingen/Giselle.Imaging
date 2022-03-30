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

        public PNGRawChunk(PngChunkStream stream)
        {
            this.Type = stream.Type;

            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                this.Data = ms.ToArray();
            }

        }

        public void Read(DataProcessor processor)
        {
            var length = processor.ReadInt();
            this.TypeRaw = processor.ReadBytes(TypeLength);
            this.Data = processor.ReadBytes(length);

            var ccrc = this.CRC32;
            var rcrc = processor.ReadUInt();

            if (ccrc != rcrc)
            {
                throw new CRCException($"Read CRC are mismatch with Calculcated CRC - {rcrc} vs {ccrc}");
            }

        }

        public void Write(DataProcessor processor)
        {
            processor.WriteInt(this.Data.Length);
            processor.WriteBytes(this.TypeRaw);
            processor.WriteBytes(this.Data);
            processor.WriteUInt(this.CRC32);
        }

        public uint CRC32
        {
            get
            {
                var crc = CRCUtils.CRC32Seed;
                crc = CRCUtils.AccumulateCRC32(crc, this.TypeRaw);
                crc = CRCUtils.AccumulateCRC32(crc, this.Data);
                return CRCUtils.FinalizeCalculateCRC32(crc);
            }

        }

    }

}
