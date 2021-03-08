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
    abstract class XmlNodeSelectorBase : MarkupExtension, IValueConverter
    {
        public string XPath { get; set; }

        public XmlNodeSelectorBase()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is XmlNode node)
            {
                return this.SelectImpl(node);
            }
            else
            {
                return value;
            }
        }

        protected abstract object SelectImpl(XmlNode node);

        protected object PrepareItem(XmlNode node)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Attribute:
                    return ((XmlAttribute)node).Value;
                case XmlNodeType.Text: 
                    return ((XmlText)node).Value;
                case XmlNodeType.CDATA: 
                    return ((XmlCDataSection)node).Value;
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
                    return node;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    class XmlNodesSelector : XmlNodeSelectorBase
    {
        protected override object SelectImpl(XmlNode node)
        {
            return node.SelectNodes(this.XPath).OfType<XmlNode>().Select(n => this.PrepareItem(n)).ToArray();
        }
    }

    class XmlNodeSelector : XmlNodeSelectorBase
    {
        protected override object SelectImpl(XmlNode node)
        {
            return this.PrepareItem(node.SelectSingleNode(this.XPath));
        }
    }
}
