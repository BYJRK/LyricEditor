using System;
using System.Globalization;
using System.Windows;

namespace LyricEditor.Converters
{
    public class BooleanToVisibilityConverter : BaseValueConverter
    {
        public bool Reverse { get; set; }
        public bool UseHidden { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var b = (bool)value;
            if (Reverse) b = !b;
            if (b) return Visibility.Visible;
            else return UseHidden ? Visibility.Hidden : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
