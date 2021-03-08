using Nostalgia.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Nostalgia.Controls
{
    public abstract class FileSystemItemInfo : INotifyPropertyChanged
    {
        public FileSystemInfo Info { get; }

        public FileSystemItemInfo(FileSystemInfo info)
        {
            this.Info = info;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaizePropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public abstract void Refresh();
    }

    public class FileItemInfo : FileSystemItemInfo
    {
        public FileInfo FileInfo { get; }

        public FileItemInfo(FileInfo info)
            : base(info)
        {
            this.FileInfo = info;
        }
        public override void Refresh()
        {
            this.Info.Refresh();
            this.RaizePropertyChanged("Info");
            this.RaizePropertyChanged("FileInfo");
        }
    }

    public class FolderItemInfo : FileSystemItemInfo
    {
        //public ObservableCollection<FileItemInfo> Files { get; } = new ObservableCollection<FileItemInfo>();
        //public ObservableCollection<FolderItemInfo> Folders { get; } = new ObservableCollection<FolderItemInfo>();

        //private bool _isExpanded = false;
        //public bool IsExpanded
        //{
        //    get { return _isExpanded; }
        //    set
        //    {
        //        if (_isExpanded && !value)
        //            this.OnCollapse();
        //        else if (!_isExpanded && value)
        //            this.OnExpand();

        //        _isExpanded = value;
        //    }
        //}

        //private void OnCollapse()
        //{
        //    throw new NotImplementedException();
        //}

        //private void OnExpand()
        //{
        //    throw new NotImplementedException();
        //}

        public DirectoryInfo DirInfo { get; }

        public FolderItemInfo(DirectoryInfo info)
            : base(info)
        {
            this.DirInfo = info;
        }
        public override void Refresh()
        {
            this.Info.Refresh();
            this.RaizePropertyChanged("Info");
            this.RaizePropertyChanged("DirInfo");
        }
    }

    public class DriveItemInfo : FolderItemInfo
    {
        public DriveInfo DriveInfo { get; private set; }

        public string VolumeDescription { get; private set; }

        public ImageSource Icon { get; private set; }

        public DriveItemInfo(string drivePath)
            : base(new DirectoryInfo(drivePath))

        {
            this.Refresh();
        }

        public override void Refresh()
        {
            this.DriveInfo = new DriveInfo(Path.GetPathRoot(this.DirInfo.FullName));

            this.Icon = FsInfoFormat.GetFsItemIcon(this.DirInfo.FullName, true, out var descr);
            if (string.IsNullOrWhiteSpace(this.DriveInfo.VolumeLabel))
                this.VolumeDescription = descr;
            else
                this.VolumeDescription = this.DriveInfo.VolumeLabel + ":" + descr;


            this.RaizePropertyChanged("DriveInfo");
            this.RaizePropertyChanged("VolumeDescription");
            this.RaizePropertyChanged("Icon");

            base.Refresh();
        }
    }

    public class FileSystemDataProvider : DataSourceProviderBase
    {
        // readonly FileSystemWatcher _watcher;

        readonly ObservableCollection<DriveItemInfo> _driveItems = new ObservableCollection<DriveItemInfo>();

        public FileSystemDataProvider()
        {
            //_watcher = new FileSystemWatcher()
            //{
            //    EnableRaisingEvents = false,
            //    IncludeSubdirectories = false,
            //    Path = Environment.CurrentDirectory,
            //    NotifyFilter = NotifyFilters.LastWrite
            //};
        }

        protected override object ObtainDataImpl()
        {
            this.SyncFsRoots();

            return _driveItems;
        }

        private void SyncFsRoots()
        {
            var newDrives = Environment.GetLogicalDrives().ToList();
            var oldDrives = _driveItems.ToList();

            foreach (var oldDrive in oldDrives.ToArray())
            {
                var newDrive = newDrives.FirstOrDefault(d => d == oldDrive.DriveInfo.Name);
                if (newDrive != null)
                {
                    oldDrives.Remove(oldDrive);
                    newDrives.Remove(newDrive);
                }
            }

            foreach (var drive in oldDrives)
            {
                _driveItems.Remove(drive);
            }

            foreach (var drive in newDrives)
            {
                var item = new DriveItemInfo(drive);

                var index = _driveItems.InternalBinarySearch(item, (a, b) => a.DriveInfo.Name.CompareTo(b.DriveInfo.Name));
                if (index < 0)
                    index = ~index;

                _driveItems.Insert(index, item);
            }
        }
    }
}
