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
        private readonly Queue<StorageFile> q = new Queue<StorageFile>();
        private bool copying = false;

        private int _copyValue = 0;
        private int _maximumValue = 0;

        public event EventHandler<int> CopyValueChanged;
        public event EventHandler<int> MaximumValueChanged;

        public FileCopyService()
        {
            Task copyTask = new Task(copyThread);
            copyTask.Start();
        }

        public void Enqueue(StorageFile f)
        {
            lock (q)
            {
                if (!copying)
                {
                    DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal,
                        () => {
                            _copyValue = 0;
                            _maximumValue = 0;
                            CopyValueChanged?.Invoke(this, _copyValue);
                            Locator.FileExplorerVM.copyStarted();
                        });
                }

                q.Enqueue(f);
                copying = true;
                Monitor.Pulse(q);
            }

            _maximumValue++;
            MaximumValueChanged?.Invoke(this, _maximumValue);
        }

        private async void copyThread()
        {
            while (true)
            {
                StorageFile f = null;
                lock (q)
                {
                    if (q.Count == 0)
                        Monitor.Wait(q);

                    if (q.Count > 0)
                        f = q.Dequeue();
                }

                if (f != null)
                {
                    LogHelper.Log("Copying file: " + f.Path);
                    StorageFile copy = await f.CopyAsync(await FileUtils.GetLocalStorageMediaFolder(),
                        f.Name, NameCollisionOption.GenerateUniqueName);
                    bool success = await Locator.MediaLibrary.DiscoverMediaItemOrWaitAsync(copy, false);
                    if (success == false)
                        await copy.DeleteAsync();

                    _copyValue++;
                    CopyValueChanged?.Invoke(this, _copyValue);

                    lock (q)
                    {
                        if (q.Count == 0)
                        {
                            copying = false;
                            DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal,
                                () => Locator.FileExplorerVM.copyEnded());
                        }
                    }
                }
            }
        }
    }
}
