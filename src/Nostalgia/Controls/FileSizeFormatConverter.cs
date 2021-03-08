using Nostalgia.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Nostalgia.Controls
{
    class FileSizeFormatConverter : MarkupExtension, IValueConverter
    {
        public FileSizeFormatConverter()
        {

        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (long)Convert.ChangeType(value, typeof(long));
            var str = FsInfoFormat.FormatFileSize(size);
            return str;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
