using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec
{
    public interface ISaveOptions
    {
        ISaveOptions Clone();
    }

    public interface ISaveOptions<T> : ISaveOptions where T : ISaveOptions<T>
    {
        new T Clone();
    }

    public abstract class SaveOptions<T> : ISaveOptions<T> where T : ISaveOptions<T>, new()
    {
        public SaveOptions()
        {

        }

        public SaveOptions(T other)
        {

        }

        public abstract T Clone();

        ISaveOptions ISaveOptions.Clone() => this.Clone();
    }

}
