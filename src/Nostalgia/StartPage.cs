using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Nostalgia
{
    public partial class StartPage : Grid
    {
        public StartPage()
        {
            InitializeComponent();
        }

        public async void btnNewProject_Click(object sender, EventArgs ea)
        {
            ////// Command="{x:Static ss:VSCommands.ExecuteCommand}" CommandParameter="File.NewProject"
            ////var commandService = await ServiceProvider.GetGlobalServiceAsync<OleMenuCommandService, IMenuCommandService>();
            //////commandService.GlobalInvoke(StartPageWindowCommand.Instance.MenuCommandID);
            ////commandService.GlobalInvoke(VSCommand);
            
            var dte = (DTE2)await ServiceProvider.GetGlobalServiceAsync(typeof(DTE));

            if (dte != null)
            {
                dte.ExecuteCommand("File.NewProject");
            }
        }
    }
}
