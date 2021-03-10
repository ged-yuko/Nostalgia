using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xml;

namespace Nostalgia.Controls
{
    public class UnXmlConverter : MarkupExtension, IValueConverter
    {
        public UnXmlConverter()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case XmlAttribute node:
                    return node.Value;
                case XmlText node: 
                    return node.Value;
                case XmlCDataSection node:
                    return node.Value;
                case XmlNodeType.Whitespace:
                case XmlNodeType.SignificantWhitespace:
                    return string.Empty;
                case XmlNodeType.None:
                case XmlNodeType.Element:
                case XmlNodeType.Entity:
                case XmlNodeType.EntityReference:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.Comment:
                case XmlNodeType.Document:
                case XmlNodeType.DocumentType:
                case XmlNodeType.DocumentFragment:
                case XmlNodeType.Notation:
                case XmlNodeType.EndElement:
                case XmlNodeType.EndEntity:
                case XmlNodeType.XmlDeclaration:
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
