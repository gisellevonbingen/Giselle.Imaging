using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec
{
    public static class SaveOptionsExtensions
    {
        public static T CastOrDefault<T>(this SaveOptions options) where T : SaveOptions, new() => options as T ?? new T();

    }

}
