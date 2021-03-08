using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.CodeContainerManagement;
using Microsoft.VisualStudio.Shell.Interop;
using Nostalgia.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Nostalgia.Controls
{
    public class RecentProject : DependencyObject
    {
        private string _path;
        private CodeContainer _info;

        public FileInfo Info { get; private set; }

        public bool IsSourceControlled { get { return _info.IsSourceControlled; } }
        public DateTimeOffset LastAccessed { get { return _info.LastAccessed; } }

        #region DateClassificationCategory ClassificationCategory 

        public DateClassificationCategory ClassificationCategory
        {
            get { return (DateClassificationCategory)GetValue(ClassificationCategoryProperty); }
            private set { SetValue(ClassificationCategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClassificationCategory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClassificationCategoryProperty =
            DependencyProperty.Register("ClassificationCategory", typeof(DateClassificationCategory), typeof(RecentProject), new UIPropertyMetadata(default(DateClassificationCategory)));

        #endregion

        #region bool IsFavorite 

        public bool IsFavorite
        {
            get { return (bool)GetValue(IsFavoriteProperty); }
            set { SetValue(IsFavoriteProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFavorite.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFavoriteProperty =
            DependencyProperty.Register("IsFavorite", typeof(bool), typeof(RecentProject), new UIPropertyMetadata(default(bool)));

        #endregion

        private string _description = null;
        public string Description { get { return _description ?? (_description = FsInfoFormat.GetFsItemDescription(_path)); } }


        private ImageSource _largeIcon = null, _smallIcon = null;

        public ImageSource SmallIcon { get { return _smallIcon ?? (_smallIcon = FsInfoFormat.GetFsItemIcon(_path, true)); } }
        public ImageSource LargeIcon { get { return _largeIcon ?? (_largeIcon = FsInfoFormat.GetFsItemIcon(_path, false)); } }

        public RecentProject(string path, CodeContainer info)
        {
            _path = path;
            _info = info;

            this.Info = new FileInfo(path);
            this.IsFavorite = _info.IsFavorite;
            this.ClassificationCategory = DateClassificationCategory.Get(this);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == IsFavoriteProperty)
            {
                this.ClassificationCategory = DateClassificationCategory.Get(this); 
            }

            base.OnPropertyChanged(e);
        }
    }

    public class RecentProjectsDataProvider : DataSourceProviderBase
    {

        readonly ObservableCollection<RecentProject> _data = new ObservableCollection<RecentProject>();


        public RecentProjectsDataProvider()
        {
        }


        // TODO listen for updates from VS itself
        //class ObservableSettingsCollection<T> : ObservableCollection<T>, ITargetBlock<StatefulReadOnlyList<T, object>>, IDisposable
        //     where T : class, IComparable<T>
        //{
        //    readonly ObservableSettingsListBase<T> _list;
        //    readonly Task<IDisposable> _subscription;
        //    readonly Dispatcher _dispatcher;
        //    readonly Comparison<T> _comparison;

        //    public ObservableSettingsCollection(ObservableSettingsListBase<T> list, System.Windows.Threading.Dispatcher dispatcher, Comparison<T> comparison = null)
        //    {
        //        _list = list;
        //        _subscription = list.SubscribeAsync(this);
        //        _comparison = comparison;
        //    }

        //    public System.Threading.Tasks.Task Completion { get; private set; }

        //    void IDataflowBlock.Complete() { }

        //    void IDataflowBlock.Fault(Exception exception) { }

        //    DataflowMessageStatus ITargetBlock<StatefulReadOnlyList<T, object>>.OfferMessage(DataflowMessageHeader messageHeader, 
        //                                                                                     StatefulReadOnlyList<T, object> messageValue,
        //                                                                                     ISourceBlock<StatefulReadOnlyList<T, object>> source, 
        //                                                                                     bool consumeToAccept)
        //    {
        //        if (source != null && consumeToAccept)
        //        {
        //            messageValue = source.ConsumeMessage(messageHeader, this, out var messageConsumed);
        //            if (!messageConsumed)
        //                throw new InvalidOperationException();
        //        }

        //        foreach (var item in messageValue.List)
        //        {
        //            _dispatcher.InvokeAsync(() => this.AddInternal(item));
        //            this.Add(item);
        //        }

        //        return DataflowMessageStatus.Accepted;
        //    }

        //    void AddInternal(T item)
        //    {
        //        if (_comparison != null)
        //        {
        //            var index = this.InternalBinarySearch(item, _comparison);
        //            if (index >= 0)
        //                index = ~index;

        //            this.Insert(index, item);
        //        }
        //        else
        //        {
        //            this.Add(item);
        //        }
        //    }

        //    public void Dispose()
        //    {
        //        _subscription.ContinueWith(r => r.Dispose());
        //    }
        //}

        //ObservableSettingsCollection<T> WrapSettingsList<T>(ObservableSettingsListBase<T> list)
        //     where T : class, IComparable<T>
        //{
        //    return new ObservableSettingsCollection<T>(list, this.Dispatcher);
        //}

        protected override object ObtainDataImpl()
        {
            var manager = Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsSettingsPersistenceManager)) as ISettingsManager;

            var registry = new CodeContainerRegistry(manager);

            var list = manager.GetOrCreateList("CodeContainers.Offline", isMachineLocal: true);


            var items = list.Keys.Select(k => (pth: k, c: registry.GetAsync(k).Result)).OrderBy(x => x.c, _codeContainerComparer).ToList();

            _data.Clear();
            foreach (var item in items)
            {
                _data.Add(new RecentProject(item.pth, item.c));
            }

            // var osc = WrapSettingsList(registry);


            //Guid guid = VSConstants.MruList.Projects;

            //// IVsMRUItemsStore mruobj = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsMRUItemsStore)) as IVsMRUItemsStore;
            //IVsMRUItemsStore mru = ServiceProvider.GlobalProvider.GetService(typeof(SVsMRUItemsStore)) as IVsMRUItemsStore;
            //string[] mruItems = new string[10];
            //uint itemCount = mru.GetMRUItems(ref guid, "", 10, mruItems);
            //var mruList = new List<string>();
            //for (int i = 0; i < itemCount; i++)
            //{
            //    string[] subValue = mruItems[i].Split('|');
            //    mruList.Add(subValue[0]);
            //}

            //return mruList;

            return _data;
        }

        static Comparer<CodeContainer> _codeContainerComparer = Comparer<CodeContainer>.Create(CodeContainerComparison);

        static int CodeContainerComparison(CodeContainer a, CodeContainer b)
        {
            if (a.IsFavorite == b.IsFavorite)
                return a.LastAccessed.CompareTo(b.LastAccessed);

            if (a.IsFavorite)
                return -1;
            else
                return 1;
        }

        //private void Clear(Func<string, bool> shouldDelete)
        //{
        //    var manager = GetManager();
        //    var recents = GetRecents(manager);

        //    if (recents.Count == 0) { return; }

        //    var registry = new CodeContainerRegistry(manager);

        //    foreach (var path in recents)
        //    {
        //        if (shouldDelete(path))
        //        {
        //            registry.RemoveAsync(path);
        //        }
        //    }
        //}

        [Guid("9b164e40-c3a2-4363-9bc5-eb4039def653")]
        private class SVsSettingsPersistenceManager
        {
        }
    }
}
