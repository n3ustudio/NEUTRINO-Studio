using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeutrinoStudio.Shell.Models
{
    public enum OutputFormat
    {
        Undefined = 0,
        Wav = 1,
        Mp3 = 2,
        Flac = 3
    }

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
