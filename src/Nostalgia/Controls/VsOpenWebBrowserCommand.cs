using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace Nostalgia.Controls
{
    public class VsOpenWebBrowserCommand : MarkupExtension, ICommand
    {
        public VsOpenWebBrowserCommand()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            IVsWindowFrame ppFrame;
            var service = Package.GetGlobalService(typeof(IVsWebBrowsingService)) as IVsWebBrowsingService;
            service.Navigate((parameter ?? string.Empty).ToString(), (uint)__VSWBNAVIGATEFLAGS.VSNWB_ForceNew, out ppFrame);
            // wtf is this not working
        }
    }
}
