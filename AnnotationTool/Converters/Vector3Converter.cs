using SharpDX;
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
            var result = "";

            if (value is Vector3)
            {
                var vector = (Vector3)value;

                switch ((int)parameter)
                {
                    case 0:
                        result = vector.X.ToString();
                        break;
                    case 1:
                        result = vector.Y.ToString();
                        break;
                    case 2:
                        result = vector.Z.ToString();
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
