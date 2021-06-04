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
            var result = 0f;

            if (value is Line)
            {
                var line = (Line)value;


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
            }

            if (value is SharpDX.Vector3)
            {
                var vector = (SharpDX.Vector3)value;

                switch ((int)parameter)
                {
                    case 0:
                        result = vector.X;
                        break;
                    case 1:
                        result = vector.Y;
                        break;
                    case 2:
                        result = vector.Z;
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
