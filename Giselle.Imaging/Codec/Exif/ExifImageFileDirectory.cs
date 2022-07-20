using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec.Exif
{
    public class ExifImageFileDirectory : Dictionary<ExifTagId, ExifValue>
    {
        public ExifImageFileDirectory()
        {

        }

        public ExifImageFileDirectory(IDictionary<ExifTagId, ExifValue> dictionary) : base(dictionary)
        {

        }

        public long OffsetValuesSize
        {
            get
            {
                var size = 0;

                foreach (var pair in this)
                {
                    var raw = new ExifRawEntry(pair);

                    if (raw.IsOffset == true)
                    {
                        size += raw.ValuesSize;
                    }

                }

                return size;
            }

        }

        public bool TryGetIntegers(ExifTagId id, out IExifValueIntegers integers)
        {
            if (this.TryGetValue(id, out var value) == true && value is IExifValueIntegers _integers)
            {
                integers = _integers;
                return true;
            }
            else
            {
                integers = null;
                return false;
            }

        }

        public int GetSigned(ExifTagId id, int fallback = default)
        {
            if (this.TryGetIntegers(id, out var integers) == true)
            {
                return integers.AsSigned;
            }
            else
            {
                return fallback;
            }

        }

        public uint GetUnsigned(ExifTagId id, uint fallback = default)
        {
            if (this.TryGetIntegers(id, out var integers) == true)
            {
                return integers.AsUnsigned;
            }
            else
            {
                return fallback;
            }

        }

        public int[] GetSigneds(ExifTagId id)
        {
            if (this.TryGetIntegers(id, out var integers) == true)
            {
                return integers.AsSigneds.ToArray();
            }
            else
            {
                return new int[0];
            }

        }

        public uint[] GetUnsigneds(ExifTagId id)
        {
            if (this.TryGetIntegers(id, out var integers) == true)
            {
                return integers.AsUnsigneds.ToArray();
            }
            else
            {
                return new uint[0];
            }

        }

        public void SetLong(ExifTagId id, uint value) => this[id] = new ExifValueLongs() { Value = value };

        public void SetLongs(ExifTagId id, uint[] values) => this[id] = new ExifValueLongs() { Values = values };

        public void SetSLong(ExifTagId id, int value) => this[id] = new ExifValueSLongs() { Value = value };

        public void SetSLongs(ExifTagId id, int[] values) => this[id] = new ExifValueSLongs() { Values = values };

        public void SetShort(ExifTagId id, ushort value) => this[id] = new ExifValueShorts() { Value = value };

        public void SetShorts(ExifTagId id, ushort[] values) => this[id] = new ExifValueShorts() { Values = values };

        public void SetSShort(ExifTagId id, short value) => this[id] = new ExifValueSShorts() { Value = value };

        public void SetSShorts(ExifTagId id, short[] values) => this[id] = new ExifValueSShorts() { Values = values };

        public void SetRational(ExifTagId id, ExifRational value) => this[id] = new ExifValueRationals() { Value = value };

        public void SetRationals(ExifTagId id, ExifRational[] values) => this[id] = new ExifValueRationals() { Values = values };

        public void SetSRational(ExifTagId id, ExifSRational value) => this[id] = new ExifValueSRationals() { Value = value };

        public void SetSRationals(ExifTagId id, ExifSRational[] values) => this[id] = new ExifValueSRationals() { Values = values };

    }

}
