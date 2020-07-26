using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace BorderCrossing.Extensions
{
    public static class Extensions
    {
        public static DateTime ToDateTime(this string ticks)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(long.Parse(ticks));
            return new DateTime(1970, 1, 1) + time;
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>()
                ?.GetName();
        }
    }
}