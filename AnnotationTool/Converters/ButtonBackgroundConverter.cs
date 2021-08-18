using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AnnotationTool.Converters
{
    class ButtonBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            if (!(values[0] is string) || !(values[1] is string))
            {
                return new SolidColorBrush(Colors.Gray);
            }

            var equals = String.Equals((string)values[0], (string)values[1]);
            return equals ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Gray);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
