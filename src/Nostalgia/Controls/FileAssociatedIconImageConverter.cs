using Nostalgia.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Nostalgia.Controls
{
    public class FileAssociatedIconImageConverter : MarkupExtension, IValueConverter
    {
        public bool Small { get; set; }
        public ImageSource DefaultImage { get; set; }

        public FileAssociatedIconImageConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string filePath && File.Exists(filePath))
            {
                return FsInfoFormat.GetFsItemIcon(filePath, this.Small, out var descr);
            }
            else
            {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

    }
}
