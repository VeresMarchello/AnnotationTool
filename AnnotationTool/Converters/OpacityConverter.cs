using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace AnnotationTool.Converters
{
    class OpacityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var string1 = values[0].ToString();
            var string2 = values[1].ToString();

            string common = string.Concat(string1.TakeWhile((c, i) => c == string2[i]));
            return common.Length > 0 ? 1 : 0.7;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
