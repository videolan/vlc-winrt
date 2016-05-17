using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VLC_WinRT.Commands.VLCFileExplorer;
using VLC_WinRT.Model;
using VLC_WinRT.Model.FileExplorer;
using VLC_WinRT.Utils;
using Windows.UI.Xaml;

namespace VLC_WinRT.ViewModels.Others.VlcExplorer
{
    public abstract class FileExplorer : BindableBase
    {
        #region private props
        private bool _isFolderEmpty;
        private bool _isLoadingFiles;
        private RootFolderType type;
        #endregion

        #region private fields
        private ObservableCollection<VLCStorageFolder> _backStack = new ObservableCollection<VLCStorageFolder>();
        private ObservableCollection<IVLCStorageItem> _storageItems = new ObservableCollection<IVLCStorageItem>();
        #endregion

        #region public props
        public string Id;
        public string Name { get; set; }
        public string ArtworkUrl { get; set; }

        public IStorageItemClickedCommand NavigateToCommand { get; } = new IStorageItemClickedCommand();

        public PlayFolderCommand PlayFolderCommand { get; } = new PlayFolderCommand();

        #endregion

        #region public props
        public bool IsFolderEmpty
        {
            get { return _isFolderEmpty; }
            set { SetProperty(ref _isFolderEmpty, value); }
        }

        public string CurrentFolderName
        {
            get
            {
                OnPropertyChanged(nameof(PreviousFolderName));
                return BackStack.Last().Name;
            }
        }

        public string PreviousFolderName
        {
            get
            {
                if (BackStack.Count > 1)
                {
                    return BackStack.ElementAt(BackStack.Count - 2).Name;
                }
                return Strings.Home;
            }
        }

        public RootFolderType Type
        { get; private set; }

        public bool IsLoadingFiles
        {
            get { return _isLoadingFiles; }
            set
            {
                SetProperty(ref _isLoadingFiles, value);
                OnPropertyChanged(nameof(LoadingBarVisibility));
            }
        }

        public Visibility LoadingBarVisibility
        {
            get { return IsLoadingFiles ? Visibility.Visible : Visibility.Collapsed; }
        }

        #endregion

        #region public fields
        public ObservableCollection<IVLCStorageItem> StorageItems
        {
            get { return _storageItems; }
            set { SetProperty(ref _storageItems, value); }
        }

        public ObservableCollection<VLCStorageFolder> BackStack
        {
            get { return _backStack; }
            set { SetProperty(ref _backStack, value); }
        }
        #endregion

        public FileExplorer(string name, RootFolderType type)
        {
            Type = type;
            Name = name;
        }

        public abstract Task GetFiles();
        public abstract Task NavigateTo(IVLCStorageItem storageItem);
        public void GoBack()
        {
            if (BackStack.Count > 1)
            {
                BackStack.Remove(BackStack.Last());
                Task.Run(() => GetFiles());
                OnPropertyChanged(nameof(CurrentFolderName));
            }
        }
    }
}
