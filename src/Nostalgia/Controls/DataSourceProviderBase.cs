using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Nostalgia.Controls
{
    public abstract class DataSourceProviderBase : DataSourceProvider
    {
        public bool IsAsynchronous { get; set; }

        public DataSourceProviderBase()
        {
        }

        protected sealed override void BeginQuery()
        {
            if (this.IsAsynchronous)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(PrepareDataProc));
            }
            else
            {
                PrepareDataProc(null);
            }
        }

        void PrepareDataProc(object obj)
        {
            try
            {
                var data = this.ObtainDataImpl();
                base.OnQueryFinished(data);
            }
            catch (Exception ex)
            {
                base.OnQueryFinished(null, ex, null, null);
            }
        }

        protected abstract object ObtainDataImpl();

    }


}
