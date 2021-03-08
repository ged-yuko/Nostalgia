using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nostalgia.Controls
{
    class ProjectTemplatesDataProvider : DataSourceProviderBase
    {
        public ProjectTemplatesDataProvider()
        {
        }

        protected override object ObtainDataImpl()
        {

            //var dte = (DTE2)await ServiceProvider.GetGlobalServiceAsync(typeof(DTE));
            //EnvDTE90.Templates

            throw new NotImplementedException("");
        }
    }
}
