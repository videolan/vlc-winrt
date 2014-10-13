/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.DataRepository;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Helpers.MusicLibrary.xboxmusic.Models;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Views.UserControls;
using XboxMusicLibrary;
using VLC_WINRT_APP.Commands.Music;
using Panel = VLC_WINRT_APP.Model.Panel;

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicLibraryVM : BindableBase
    {
        public static ArtistDataRepository _artistDataRepository = new ArtistDataRepository();
        public static TrackDataRepository _trackDataRepository = new TrackDataRepository();
        public static AlbumDataRepository _albumDataRepository = new AlbumDataRepository();
        #region private fields
#if WINDOWS_APP
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
#endif
        private ObservableCollection<ArtistItem> _artistses = new ObservableCollection<ArtistItem>();
        private ObservableCollection<string> _albumsCover = new ObservableCollection<string>();
        private ObservableCollection<TrackItem> _trackses = new ObservableCollection<TrackItem>();
        private ObservableCollection<AlbumItem> _favoriteAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _randomAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _albums = new ObservableCollection<AlbumItem>(); 
        #endregion
        #region private props
        private SidebarState _sidebarState;
        private LoadingState _loadingState;
        private ArtistAlbumsSemanticZoomInvertZoomCommand _artistAlbumsSemanticZoomInvertZoomCommand;
        private ChangeAlbumArtCommand _changeAlbumArtCommand;
        private DownloadAlbumArtCommand _downloadAlbumArtCommand;
        private AlbumClickedCommand _albumClickedCommand;
        private ArtistClickedCommand _artistClickedCommand;
        private TrackClickedCommand _trackClickedCommand;
        private ArtistItem _currentArtist;
        private bool _isLoaded = false;
        private bool _isBusy = false;
        private bool _isMusicLibraryEmpty = true;
        private bool _isAlbumPageShown = false;
        private string _currentIndexingStatus = "";
        private bool _isMainPageMusicArtistAlbumsSemanticZoomViewedIn;
        #endregion
        #region public fields
        public ObservableCollection<AlbumItem> FavoriteAlbums
        {
            get { return _favoriteAlbums; }
            set { SetProperty(ref _favoriteAlbums, value); }
        }

        public ObservableCollection<AlbumItem> RandomAlbums
        {
            get { return _randomAlbums; }
            set { SetProperty(ref _randomAlbums, value); }
        }

#if WINDOWS_APP
        public ObservableCollection<Panel> Panels
        {
            get { return _panels; }
            set
            {
                SetProperty(ref _panels, value);
            }
        }
#endif

        public ObservableCollection<ArtistItem> Artists
        {
            get { return _artistses; }
            set { SetProperty(ref _artistses, value); }
        }

        public ObservableCollection<TrackItem> Tracks
        {
            get { return _trackses; }
            set
            {
                SetProperty(ref _trackses, value);
            }
        }

        public ObservableCollection<AlbumItem> Albums
        {
            get { return _albums; }
            set { SetProperty(ref _albums, value); }
        }

        #endregion
        #region public props
        public SidebarState SidebarState
        {
            get { return _sidebarState; }
            set { SetProperty(ref _sidebarState, value); }
        }
        public LoadingState LoadingState
        {
            get { return _loadingState; }
            set { SetProperty(ref _loadingState, value); }
        }

        public string CurrentIndexingStatus
        {
            get { return _currentIndexingStatus; }
            set { SetProperty(ref _currentIndexingStatus, value); }
        }

        public bool IsAlbumPageShown
        {
            get { return _isAlbumPageShown; }
            set { SetProperty(ref _isAlbumPageShown, value); }
        }
        public bool IsMainPageMusicArtistAlbumsSemanticZoomViewedIn
        {
            get { return _isMainPageMusicArtistAlbumsSemanticZoomViewedIn; }
            set { SetProperty(ref _isMainPageMusicArtistAlbumsSemanticZoomViewedIn, value); }
        }
        public bool IsLoaded
        {
            get { return _isLoaded; }
            set { SetProperty(ref _isLoaded, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public bool IsMusicLibraryEmpty
        {
            get { return _isMusicLibraryEmpty; }
            set { SetProperty(ref _isMusicLibraryEmpty, value); }
        }
        public ArtistAlbumsSemanticZoomInvertZoomCommand ArtistAlbumsSemanticZoomInvertZoomCommand
        {
            get { return _artistAlbumsSemanticZoomInvertZoomCommand; }
            set { SetProperty(ref _artistAlbumsSemanticZoomInvertZoomCommand, value); }
        }

        public ChangeAlbumArtCommand ChangeAlbumArtCommand
        {
            get
            {
                return _changeAlbumArtCommand;
            }
            set { SetProperty(ref _changeAlbumArtCommand, value); }
        }

        public DownloadAlbumArtCommand DownloadAlbumArtCommand
        {
            get
            {
                return _downloadAlbumArtCommand;
            }
            set { SetProperty(ref _downloadAlbumArtCommand, value); }
        }

        public AlbumClickedCommand AlbumClickedCommand
        {
            get
            {
                return _albumClickedCommand;
            }
            set { SetProperty(ref _albumClickedCommand, value); }
        }

        public ArtistClickedCommand ArtistClickedCommand
        {
            get
            {
                return _artistClickedCommand;
            }
            set { SetProperty(ref _artistClickedCommand, value); }
        }

        public TrackClickedCommand TrackClickedCommand
        {
            get { return _trackClickedCommand; }
            set { SetProperty(ref _trackClickedCommand, value); }
        }

        public ArtistItem CurrentArtist
        {
            get { return _currentArtist; }
            set { SetProperty(ref _currentArtist, value); }
        }
        #endregion
        #region XBOX Music Stuff
        public MusicHelper XboxMusicHelper = new MusicHelper();
        public Authenication XboxMusicAuthenication;

        #endregion
        public MusicLibraryVM()
        {
            LoadingState = LoadingState.NotLoaded;
            var resourceLoader = new ResourceLoader();
#if WINDOWS_APP
            Panels.Add(new Panel(resourceLoader.GetString("Albums").ToLower(), 0, 1, App.Current.Resources["HomePath"].ToString(), true));
            Panels.Add(new Panel(resourceLoader.GetString("Artists").ToLower(), 1, 0.4, App.Current.Resources["HomePath"].ToString()));
            Panels.Add(new Panel(resourceLoader.GetString("Songs").ToLower(), 2, 0.4, App.Current.Resources["HomePath"].ToString()));
            //Panels.Add(new Panel(resourceLoader.GetString("Pinned").ToLower(), 2, 0.4, App.Current.Resources["HomePath"].ToString()));
            //Panels.Add(new Panel(resourceLoader.GetString("Playlists").ToLower(), 2, 0.4, App.Current.Resources["HomePath"].ToString()));
#endif
        }

        public void Initialize()
        {
            _albumClickedCommand = new AlbumClickedCommand();
            _artistClickedCommand = new ArtistClickedCommand();
            _trackClickedCommand = new TrackClickedCommand();
            _changeAlbumArtCommand = new ChangeAlbumArtCommand();
            _downloadAlbumArtCommand = new DownloadAlbumArtCommand();
            _artistAlbumsSemanticZoomInvertZoomCommand = new ArtistAlbumsSemanticZoomInvertZoomCommand();
            CurrentIndexingStatus = "Loading music";
            LoadingState = LoadingState.Loading;
            Task.Run(async () =>
            {
                await GetMusicFromLibrary();
                await GetFavoriteAndRandomAlbums();
            });
        }

        #region methods
        public async Task GetFavoriteAndRandomAlbums()
        {
            await MusicLibraryManagement.LoadFavoriteRandomAlbums();
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged("RandomAlbums");
                OnPropertyChanged("FavoriteAlbums");
            });
        }

        public async Task GetMusicFromLibrary()
        {
            await LoadFromDatabase();
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => IsMusicLibraryEmpty = false);
            if (!Artists.Any())
            {
                await StartIndexing();
                return;
            }
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                LoadingState = LoadingState.Loaded;
                IsLoaded = true;
                IsBusy = false;
            });
#if WINDOWS_PHONE_APP
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Albums = await _albumDataRepository.LoadAlbums(x=>x.ArtistId != 0);
            });
#endif
        }

        public async Task StartIndexing()
        {
            if (await DoesMusicDatabaseExist())
            {
                _artistDataRepository.Drop();
                _trackDataRepository.Drop();
                _albumDataRepository.Drop();
            }

            DispatchHelper.InvokeAsync(() =>
            {
                CurrentIndexingStatus = "Searching for music";
                IsBusy = true;
                IsLoaded = false;
                OnPropertyChanged("IsBusy");
                OnPropertyChanged("IsLoaded");
#if WINDOWS_PHONE_APP
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.ShowAsync();
                statusBar.ProgressIndicator.Text = "Indexing music library";
#endif
            });
            _artistDataRepository = new ArtistDataRepository();
            _artistDataRepository.Initialize();
            _trackDataRepository.Initialize();
            _albumDataRepository.Initialize();

            await MusicLibraryManagement.GetAllMusicFolders();

            await LoadFromDatabase();

            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                IsBusy = false;
                IsLoaded = true;
                IsMusicLibraryEmpty = false;
                OnPropertyChanged("Artists");
                OnPropertyChanged("FavoriteAlbums");
                OnPropertyChanged("IsBusy");
                OnPropertyChanged("IsMusicLibraryEmpty");
                OnPropertyChanged("IsLoaded");
                LoadingState = LoadingState.Loaded;
#if WINDOWS_PHONE_APP
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ProgressIndicator.HideAsync();
#endif
            });
            GetFavoriteAndRandomAlbums();
        }


        private async Task LoadFromDatabase()
        {
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Artists.Clear();
                Tracks.Clear();
            });
            await MusicLibraryManagement.LoadFromSQL();
            DispatchHelper.InvokeAsync(() =>
            {
                IsMusicLibraryEmpty = !Artists.Any();
                OnPropertyChanged("IsMusicLibraryEmpty");
            });
        }

        public void ExecuteSemanticZoom(SemanticZoom sZ, CollectionViewSource cvs)
        {
            (sZ.ZoomedOutView as ListViewBase).ItemsSource = cvs.View.CollectionGroups;
        }

        private async Task<bool> DoesMusicDatabaseExist()
        {
            return await DoesFileExistHelper.DoesFileExistAsync("mediavlc.sqlite");
        }
        #endregion
    }
}
