using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VLC.Helpers;
using VLC.Utils;
using VLC.ViewModels;
using Windows.Storage;
using Windows.UI.Core;

namespace VLC.Services.RunTime
{
    public class FileCopyService
    {
        private readonly Queue<StorageFile> filesQueue = new Queue<StorageFile>();
        private bool copying = false;

        public FileCopyService()
        {
            Task copyTask = new Task(copyThread);
            copyTask.Start();
        }

        private int _nbCopiedFiles = 0;
        private int _totalNbFiles = 0;

        private int NbCopiedFiles
        {
            get { return _nbCopiedFiles; }
            set
            {
                _nbCopiedFiles = value;
                DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal,
                    () => NbCopiedFilesChanged?.Invoke(this, _nbCopiedFiles));
            }
        }

        private int TotalNbFiles
        {
            get { return _totalNbFiles; }
            set
            {
                _totalNbFiles = value;
                DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal,
                    () => TotalNbFilesChanged?.Invoke(this, _totalNbFiles));
            }
        }

        public event EventHandler<int> NbCopiedFilesChanged;
        public event EventHandler<int> TotalNbFilesChanged;

        public event EventHandler CopyStarted;
        public event EventHandler CopyEnded;

        public void Enqueue(StorageFile f)
        {
            lock (filesQueue)
            {
                if (!copying)
                {
                    NbCopiedFiles = 0;
                    TotalNbFiles = 0;
                    DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal,
                        () => CopyStarted?.Invoke(this, null));
                }

                filesQueue.Enqueue(f);
                copying = true;
                Monitor.Pulse(filesQueue);
            }

            TotalNbFiles++;
        }

        private async void copyThread()
        {
            while (true)
            {
                StorageFile f = null;
                lock (filesQueue)
                {
                    if (filesQueue.Count == 0)
                        Monitor.Wait(filesQueue);

                    if (filesQueue.Count > 0)
                        f = filesQueue.Dequeue();
                }

                if (f != null)
                {
                    LogHelper.Log("Copying file: " + f.Path);
                    StorageFile copy = await f.CopyAsync(await FileUtils.GetLocalStorageMediaFolder(),
                        f.Name, NameCollisionOption.GenerateUniqueName);
                    bool success = await Locator.MediaLibrary.DiscoverMediaItemOrWaitAsync(copy, false);
                    if (success == false)
                        await copy.DeleteAsync();

                    NbCopiedFiles++;

                    lock (filesQueue)
                    {
                        if (filesQueue.Count == 0)
                        {
                            copying = false;
                            DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal,
                                () => CopyEnded?.Invoke(this, null));
                        }
                    }
                }
            }
        }
    }
}
