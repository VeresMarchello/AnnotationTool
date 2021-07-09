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
            var type = MarkingType.GeneralPruning;


            //if (value is _2DLine)
            //{
            //    type = ((_2DLine)value).Type;

            //}
            //else if (value is MarkingType)
            //{
            type = (MarkingType)value;
            //}

            color = ViewModelBase.GetColor(type);
            color.A = 100;

            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
