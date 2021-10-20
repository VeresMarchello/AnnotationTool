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

            try
            {
                string fullFileName = (string)value;
                if (parameter != null && int.Parse(parameter.ToString()) == 1)
                {
                    FileInfo fileInfo = new FileInfo(fullFileName);
                    fileName = fileInfo.Name;
                }
                else
                {
                    int index = fullFileName.IndexOf("Images");
                    fileName = fullFileName.Substring(index);
                }
            }
            catch (Exception e) { Console.WriteLine(e.Message); }

            return fileName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
