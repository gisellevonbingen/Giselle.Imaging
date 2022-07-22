using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.Imaging.Physical
{
    public static class PhysicalValueUtils
    {
        public static T ConvertTo<T>(this T value, PhysicalUnit toUnit) where T : IPhysicalValue, new() => new T()
        {
            Value = GetConvertValue(value, toUnit),
            Unit = toUnit,
        };

        public static T Accumulate<T>(IEnumerable<T> values, Func<IEnumerable<T>, Func<T, double?>, double?> function) where T : IPhysicalValue, new()
        {
            var unit = PhysicalUnitUtils.NormalizeUnits(values.Select(v => v.Unit).ToArray());
            return new T()
            {
                Value = function(values, v => v.GetConvertValue(unit)) ?? 0.0D,
                Unit = unit,
            };
        }

        public static T Max<T>(IEnumerable<T> values) where T : IPhysicalValue, new() => Accumulate(values, Enumerable.Max);

        public static T Min<T>(IEnumerable<T> values) where T : IPhysicalValue, new() => Accumulate(values, Enumerable.Min);

        public static double GetConvertValue(this IPhysicalValue value, PhysicalUnit toUnit)
        {
            var inch = value.Value * value.Unit.GetValuesPerInch();
            var toValue = inch / toUnit.GetValuesPerInch();
            return toValue;
        }

    }

}
