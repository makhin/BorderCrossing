using System;
using Windows.UI.Xaml.Data;
using BorderCrossing.Extensions;
using BorderCrossing.Models;

namespace BorderCrossing.UWP2
{
    public class IntervalTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var it = (IntervalType)value;
            return it.GetDisplayName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return IntervalType.Every12Hours;
            }

            foreach (IntervalType val in Enum.GetValues(typeof(IntervalType)))
            {
                if (val.GetDisplayName() == (string)value)
                {
                    return val;
                }
            }

            throw new ArgumentException("value");
        }
    }
}
