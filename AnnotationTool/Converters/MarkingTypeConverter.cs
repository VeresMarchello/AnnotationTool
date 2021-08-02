using AnnotationTool.Model;
using AnnotationTool.ViewModel;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AnnotationTool.Converters
{
    public class MarkingTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = new Color();
            var type = (MarkingType)value;
            var color4 = ViewModelBase.GetColor(type);
            color = Color.FromScRgb(color4.Alpha, color4.Red, color4.Green, color4.Blue);
            color.A = 100;

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
