using System;

namespace BorderCrossing.Extensions
{
    public static class DateExtensions
    {
        public static DateTime ToDateTime(this double ticks)
        {
            TimeSpan time = TimeSpan.FromMilliseconds(ticks);
            return new DateTime(1970, 1, 1) + time;
        }
    }
}