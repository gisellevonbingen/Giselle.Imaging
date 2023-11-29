using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Codec
{
    public static class SaveOptionsExtensions
    {
        public static T CastOrDefault<T>(this ISaveOptions options) where T : ISaveOptions, new() => CastOr(options, () => new T());

        public static T CastOr<T>(this ISaveOptions options, T fallback) where T : ISaveOptions, new() => CastOr(options, () => fallback);

        public static T CastOr<T>(this ISaveOptions options, Func<T> func) where T : ISaveOptions, new() => options is T t ? t : func();

    }

}
