using System;
using System.Globalization;
using System.Windows.Data;

namespace AnnotationTool.Converters
{
    class ImageWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var gridWith = (double)value;
            return gridWith / 3 - 2.000005;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
