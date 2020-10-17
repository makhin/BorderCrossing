using System;
using Windows.Globalization.DateTimeFormatting;
using Windows.UI.Xaml.Data;

namespace BorderCrossing.UWP2
{
    public class DateConverter2 : IValueConverter
    {
        private readonly DateTimeFormatter _shortDateFormatter = new DateTimeFormatter("shortdate");
        private readonly DateTimeFormatter _shortTimeFormatter = new DateTimeFormatter("shorttime");


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is DateTime d))
            {
                return value;
            }

            var offset = new DateTimeOffset(d.ToUniversalTime());

            var shortDate = _shortDateFormatter.Format(offset);
            var shortTime = _shortTimeFormatter.Format(offset);

            return $"{shortDate} {shortTime}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
