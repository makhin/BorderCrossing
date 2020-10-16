using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;
using BorderCrossing.Models;

namespace BorderCrossing.UWP2
{
    public class IntervalTypeConverter : IValueConverter
    {
        private readonly ResourceLoader _resourceLoader = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var it = (IntervalType)value;
            return _resourceLoader.GetString(it.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return IntervalType.Every12Hours;
            }

            foreach (IntervalType val in Enum.GetValues(typeof(IntervalType)))
            {
                if (_resourceLoader.GetString(val.ToString()) == (string)value)
                {
                    return val;
                }
            }

            throw new ArgumentException("value");
        }
    }
}
