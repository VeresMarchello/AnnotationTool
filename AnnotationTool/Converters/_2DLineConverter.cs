using AnnotationTool.Model;
using AnnotationTool.ViewModel;
using System;
using System.Globalization;
using System.Windows.Data;

namespace AnnotationTool.Converters
{
    class _2DLineConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = "";
            if (value is _2DLine && parameter is int)
            {
                var line = (_2DLine)value;

                switch ((int)parameter)
                {
                    case 0:
                        result = $"{ViewModelBase.GetMarkingTypeName(line.Type)}";
                        break;
                    case 1:
                        result = $"X:{line.FirstPoint.X} Y:{line.FirstPoint.Y}";
                        break;
                    case 2:
                        result = $"X:{line.CenterPoint.X} Y:{line.CenterPoint.Y}";
                        break;
                    case 3:
                        result = $"X:{line.MirroredPoint.X} Y:{line.MirroredPoint.Y}";
                        break;
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
