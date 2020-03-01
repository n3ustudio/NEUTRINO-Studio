using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NeutrinoStudio.Shell.Models
{
    public enum InputFormat
    {
        Undefined = 0,
        MusicXml = 1,
        Xml = 2,
        Vsq = 3,
        Vsqx = 4,
        Vpr = 5,
        Ust = 6,
        Ccs = 7
    }

    public static class InputFormatHelper
    {
        public static InputFormatConverter InputFormatConverter { get; } = new InputFormatConverter();
    }

    public class InputFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value != null && (InputFormat) value == (InputFormat) int.Parse(parameter.ToString());

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && !(bool) value) return null;
            return (InputFormat) int.Parse(parameter.ToString());
        }
    }
}
