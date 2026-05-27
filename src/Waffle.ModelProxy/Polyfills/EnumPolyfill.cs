// (c) DeNA Co., Ltd.

#pragma warning disable
namespace Polyfills;

using System;
using System.Runtime.CompilerServices;

static partial class Polyfill
{
    extension(Enum)
    {
        public static TEnum[] GetValues<TEnum>()
            where TEnum : struct, Enum
        {
            var values = Enum.GetValues(typeof(TEnum));
            var result = new TEnum[values.Length];
            Array.Copy(values, result, values.Length);
            return result;
        }

        public static bool IsDefined<TEnum>(TEnum value)
            where TEnum : struct, Enum =>
            Enum.IsDefined(typeof(TEnum), value);

        public static string[] GetNames<TEnum>()
            where TEnum : struct, Enum =>
            Enum.GetNames(typeof(TEnum));
    }
}
