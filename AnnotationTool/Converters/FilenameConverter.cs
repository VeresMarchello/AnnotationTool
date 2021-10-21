using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace AnnotationTool.Converters
{
    class FilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string fileName = "";

            string fullFileName = (string)value;

            if (!string.IsNullOrEmpty(fullFileName))
            {
                FileInfo fileInfo = new FileInfo(fullFileName);

                if (fileInfo.Exists)
                {
                    fileName = fileInfo.Name;
                }
            }

            return fileName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
