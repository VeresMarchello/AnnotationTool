using AnnotationTool.Model;
using AnnotationTool.ViewModel;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AnnotationTool.Converters
{
    class MarkingTypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string name = "";

            if (value is MarkingType)
            {
                name = ViewModelBase.GetMarkingTypeName((MarkingType)value);
            }

            return name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
