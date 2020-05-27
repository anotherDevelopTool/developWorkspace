using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using DevelopWorkspace.Base;
using System.Reactive.Linq;
namespace DevelopWorkspace.Main
{
    public class FileWatcher
    {
        private FileSystemWatcher _watcher;
        private IDisposable _disposable;

        public FileWatcher(string fileName, Action<string> action)
        {
            var file = new FileInfo(fileName);
            _watcher = new FileSystemWatcher();
            _watcher.Path = file.Directory?.FullName;
            _watcher.Filter = file.Name;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _disposable = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                h => this._watcher.Changed += h,
                h => this._watcher.Changed -= h)
                .Throttle(TimeSpan.FromSeconds(.1))
                .Subscribe(e => action(e.EventArgs.FullPath));
            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _disposable?.Dispose();
            _watcher?.Dispose();
        }
    }
}
