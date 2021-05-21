using System;
using System.Globalization;
using System.Windows.Data;
using static HelixToolkit.Wpf.SharpDX.Geometry3D;

namespace AnnotationTool.Converters
{
    public class Vector3Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var line = (Line)value;
            var result = 0f;

            
            switch ((int)parameter)
            {
                case 0:
                    result = line.P0.X;
                    break;
                case 1:
                    result = line.P0.Y;
                    break;
                case 2:
                    result = line.P1.X;
                    break;
                case 3:
                    result = line.P1.Y;
                    break;
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
