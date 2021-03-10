using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
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
    public class VsCommand : MarkupExtension, ICommand
    {
        public string CommandName { get; set; }
        public string ExtraArgs { get; set; }

        public VsCommand()
        {
        }

        public VsCommand(string cmdName)
        {
            this.CommandName = cmdName;
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
            var dte = (DTE2)Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(DTE));

            if (dte != null)
            {
                var argsStr = parameter != null ? parameter.ToString() : string.Empty;

                if (!string.IsNullOrWhiteSpace(this.ExtraArgs))
                    argsStr += this.ExtraArgs;

                dte.ExecuteCommand(this.CommandName, argsStr);
            }
        }
    }
}
