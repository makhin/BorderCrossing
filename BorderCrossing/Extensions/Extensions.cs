using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace BorderCrossing.Extensions
{
    public static class Extensions
    {
        public static DateTime ToDateTime(this long ticks)
        {
            return new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(ticks);
        }
    }
}