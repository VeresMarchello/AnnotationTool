using System;
using System.Globalization;
using System.Windows.Data;

namespace AnnotationTool.Converters
{
    class FilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string fileName = "";
            
            try
            {
                string fullFileName = (string)value;
                int index = fullFileName.IndexOf("Images");
                fileName = fullFileName.Substring(index);
            }
            catch { }

            return fileName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
