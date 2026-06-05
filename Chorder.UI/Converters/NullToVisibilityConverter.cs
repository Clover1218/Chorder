using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Chorder.UI.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool isInverse = parameter?.ToString() == "inverse";
            bool isNull = value == null || (value is string s && string.IsNullOrEmpty(s));

            if (isInverse)
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            else
                return isNull ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}