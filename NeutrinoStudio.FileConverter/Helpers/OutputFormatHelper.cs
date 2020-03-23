using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using NeutrinoStudio.FileConverter.Core;

namespace NeutrinoStudio.FileConverter.Helpers
{

    public static class OutputFormatHelper
    {
        public static OutputFormatConverter OutputFormatConverter { get; } = new OutputFormatConverter();
    }

    public class OutputFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null && (OutputFormat)value == (OutputFormat)int.Parse(parameter.ToString());

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && !(bool)value) return null;
            return (OutputFormat)int.Parse(parameter.ToString());
        }
    }

}
