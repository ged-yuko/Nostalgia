using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace Nostalgia.Controls
{
    /// <summary>
    /// Interaction logic for FeedView.xaml
    /// </summary>
    public partial class FeedView : Control
    {
        #region string DataSourceUrl 

        public string DataSourceUrl
        {
            get { return (string)GetValue(DataSourceUrlProperty); }
            set { SetValue(DataSourceUrlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataSourceUrl.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataSourceUrlProperty =
            DependencyProperty.Register("DataSourceUrl", typeof(string), typeof(FeedView), new UIPropertyMetadata(default(string)));

        #endregion

        #region XmlDocument DataDocument 

        public XmlDocument DataDocument
        {
            get { return (XmlDocument)GetValue(DataDocumentProperty); }
            private set { SetValue(DataDocumentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataDocument.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataDocumentProperty =
            DependencyProperty.Register("DataDocument", typeof(XmlDocument), typeof(FeedView), new UIPropertyMetadata(default(XmlDocument)));

        #endregion

        #region DataTemplate DataTemplate 

        public DataTemplate DataTemplate
        {
            get { return (DataTemplate)GetValue(DataTemplateProperty); }
            set { SetValue(DataTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DataTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataTemplateProperty =
            DependencyProperty.Register("DataTemplate", typeof(DataTemplate), typeof(FeedView), new UIPropertyMetadata(default(DataTemplate)));

        #endregion

        public FeedView()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == DataSourceUrlProperty && e.NewValue is string urlStr)
            {
                this.LoadData(urlStr);
            }
            
            base.OnPropertyChanged(e);
        }

        private void LoadData(string urlStr = null)
        {
            if (urlStr == null)
                urlStr = this.DataSourceUrl;

            if (!string.IsNullOrWhiteSpace(urlStr))
            {
                var doc = new XmlDocument();
                doc.Load(urlStr);

                this.DataDocument = doc;
            }
        }
    }
}