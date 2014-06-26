using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.ViewModels.Others.VlcExplorer;

namespace VLC_WINRT_APP.ViewModels.NetworkVM
{
    public class DLNAVM : BindableBase, IDisposable
    {
        #region private props

        #endregion

        #region private fields

        private ObservableCollection<FileExplorerViewModel> _dlnaVMs = new ObservableCollection<FileExplorerViewModel>();
        #endregion

        #region public props

        #endregion

        #region public fields

        public ObservableCollection<FileExplorerViewModel> DLNAVMs
        {
            get { return _dlnaVMs; }
            set { SetProperty(ref _dlnaVMs, value); }
        }
        #endregion
        public DLNAVM()
        {
            Initialize();
        }

        async Task Initialize()
        {
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                var dlnaFolder = await KnownFolders.MediaServerDevices.GetFoldersAsync();
                var tasks = new List<Task>();
                DLNAVMs.Clear();
                foreach (StorageFolder storageFolder in dlnaFolder)
                {
                    StorageFolder newFolder = storageFolder;
                    var videoLib = new FileExplorerViewModel(newFolder);
                    tasks.Add(videoLib.GetFiles());
                    DLNAVMs.Add(videoLib);
                }
                await Task.WhenAll(tasks);
            }
        }
        public void Dispose()
        {

        }
    }
}
