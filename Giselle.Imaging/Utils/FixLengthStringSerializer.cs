using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Imaging.IO;

namespace Giselle.Imaging.Utils
{
    public class FixLengthStringSerializer
    {
        public byte Suffix { get; set; } = 0;
        public bool IsLittleEndian { get; set; } = BitConverter.IsLittleEndian;

        public FixLengthStringSerializer()
        {

        }

        public int ToInt32(string value) => ToNumber(value, 4, p => p.ReadInt());

        public T ToNumber<T>(string value, int size, Func<DataProcessor, T> func)
        {
            if (string.IsNullOrEmpty(value) == true)
            {
                return default;
            }

            using (var ms = new MemoryStream())
            {
                var processor = new DataProcessor(ms) { IsLittleEndian = this.IsLittleEndian };
                processor.WriteBytes(Encoding.ASCII.GetBytes(value));

                var missings = size - processor.Length;

                if (missings > 0)
                {
                    for (var i = 0; i < missings; i++)
                    {
                        processor.WriteByte(this.Suffix);
                    }

                }

                ms.Position = 0L;
                return func(processor);
            }

        }

        public string ToString(int value) => ToString(p => p.WriteInt(value));

        public string ToString(Action<DataProcessor> action)
        {
            using (var ms = new MemoryStream())
            {
                var processor = new DataProcessor(ms) { IsLittleEndian = this.IsLittleEndian };
                action(processor);

                var bytes = ms.ToArray();
                var nullIndex = Array.IndexOf(bytes, this.Suffix);
                return Encoding.ASCII.GetString(bytes, 0, nullIndex > -1 ? nullIndex : bytes.Length);
            }

        }

    }

}
