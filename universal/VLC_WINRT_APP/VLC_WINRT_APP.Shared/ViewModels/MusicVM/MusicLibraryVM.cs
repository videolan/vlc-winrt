/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
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
using XboxMusicLibrary;
using VLC_WINRT_APP.Commands.Music;
using XboxMusicLibrary.Models;

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicLibraryVM : BindableBase
    {
        public static ArtistDataRepository _artistDataRepository = new ArtistDataRepository();
        public static TrackDataRepository _trackDataRepository = new TrackDataRepository();
        public static AlbumDataRepository _albumDataRepository = new AlbumDataRepository();
        public static TracklistItemRepository TracklistItemRepository = new TracklistItemRepository();
        public static TrackCollectionRepository TrackCollectionRepository = new TrackCollectionRepository();
        public delegate void LoadingEnded(object sender, string myValue);
        public static LoadingEnded MusicCollectionLoaded = delegate { };

        #region private fields
#if WINDOWS_APP
        private ObservableCollection<Model.Panel> _panels = new ObservableCollection<Model.Panel>();
#endif
        private ObservableCollection<ArtistItem> _artistses = new ObservableCollection<ArtistItem>();
        private ObservableCollection<TrackItem> _trackses = new ObservableCollection<TrackItem>();
        private ObservableCollection<AlbumItem> _favoriteAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _randomAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _albums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<TrackCollection> _trackCollections = new ObservableCollection<TrackCollection>();
        private IEnumerable<IGrouping<char, TrackItem>> _alphaGroupedTracks;
        #endregion
        #region private props
        private SidebarState _sidebarState;
        private LoadingState _loadingState;
        private AddToPlaylistCommand _addToPlaylistCommand;
        private TrackCollectionClickedCommand _trackCollectionClickedCommand;
        private ShowCreateNewPlaylistPane _showCreateNewPlaylistPaneCommamd;
        private ArtistAlbumsSemanticZoomInvertZoomCommand _artistAlbumsSemanticZoomInvertZoomCommand;
        private ChangeAlbumArtCommand _changeAlbumArtCommand;
        private DownloadAlbumArtCommand _downloadAlbumArtCommand;
        private AlbumClickedCommand _albumClickedCommand;
        private ArtistClickedCommand _artistClickedCommand;
        private TrackClickedCommand _trackClickedCommand;
        private PlayAllRandomCommand _playAllRandomCommand;
        private OpenAddAlbumToPlaylistDialog _openAddAlbumToPlaylistDialogCommand;
        private AlbumItem _currentAlbum;
        private ArtistItem _currentArtist;
        private TrackCollection _currentTrackCollection;
        private bool _isLoaded = false;
        private bool _isBusy = false;
        private bool _isMusicLibraryEmpty = true;
        private bool _isAlbumPageShown = false;
        private string _currentIndexingStatus = "";
        private bool _isMainPageMusicArtistAlbumsSemanticZoomViewedIn;
        #endregion

        #region public fields

        public ObservableCollection<TrackCollection> TrackCollections
        {
            get { return _trackCollections; }
            set { SetProperty(ref _trackCollections, value); }
        }

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
        public ObservableCollection<Model.Panel> Panels
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

        public IEnumerable<IGrouping<char, TrackItem>> AlphaGroupedTracks
        {
            get
            {
                return _alphaGroupedTracks;
            }
            set { SetProperty(ref _alphaGroupedTracks, value); }
        }

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

        public AddToPlaylistCommand AddToPlaylistCommand
        {
            get { return _addToPlaylistCommand ?? (_addToPlaylistCommand = new AddToPlaylistCommand()); }
        }

        public TrackCollectionClickedCommand TrackCollectionClickedCommand
        {
            get
            {
                return _trackCollectionClickedCommand ??
                       (_trackCollectionClickedCommand = new TrackCollectionClickedCommand());
            }
        }

        public ShowCreateNewPlaylistPane ShowCreateNewPlaylistPaneCommand
        {
            get
            {
                return _showCreateNewPlaylistPaneCommamd ??
                       (_showCreateNewPlaylistPaneCommamd = new ShowCreateNewPlaylistPane());
            }
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

        public PlayAllRandomCommand PlayAllRandomCommand
        {
            get { return _playAllRandomCommand; }
            set { SetProperty(ref _playAllRandomCommand, value); }
        }

        public OpenAddAlbumToPlaylistDialog OpenAddAlbumToPlaylistDialogCommand
        {
            get
            {
                return _openAddAlbumToPlaylistDialogCommand ??
                       (_openAddAlbumToPlaylistDialogCommand = new OpenAddAlbumToPlaylistDialog());
            }
            set { SetProperty(ref _openAddAlbumToPlaylistDialogCommand, value); }
        }

        public ArtistItem CurrentArtist
        {
            get
            {
                return _currentArtist;
            }
            set { SetProperty(ref _currentArtist, value); }
        }

        public AlbumItem CurrentAlbum
        {
            get { return _currentAlbum; }
            set
            {
                SetProperty(ref _currentAlbum, value);
                OnPropertyChanged("CurrentArtist");
            }
        }

        public TrackCollection CurrentTrackCollection
        {
            get { return _currentTrackCollection; }
            set { SetProperty(ref _currentTrackCollection, value); }
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
            Panels.Add(new Model.Panel(resourceLoader.GetString("Albums").ToLower(), 0, 1, App.Current.Resources["HomePath"].ToString(), true));
            Panels.Add(new Model.Panel(resourceLoader.GetString("Artists").ToLower(), 1, 0.4, App.Current.Resources["HomePath"].ToString()));
            Panels.Add(new Model.Panel(resourceLoader.GetString("Songs").ToLower(), 2, 0.4, App.Current.Resources["HomePath"].ToString()));
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
            _playAllRandomCommand = new PlayAllRandomCommand();
            CurrentIndexingStatus = "Loading music";
            LoadingState = LoadingState.Loading;
            Task.Run(async () =>
            {
                await GetFavoriteAndRandomAlbums();
                await GetMusicFromLibrary();
            });
        }

        #region methods

        public async Task GetFavoriteAndRandomAlbums()
        {
            await MusicLibraryManagement.LoadFavoriteRandomAlbums();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged("RandomAlbums");
                OnPropertyChanged("FavoriteAlbums");
            });
        }

        public async Task GetMusicFromLibrary()
        {
            await LoadFromDatabase();
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Albums = await _albumDataRepository.LoadAlbums(x => x.ArtistId != 0);
                AlphaGroupedTracks = _trackses.OrderBy(x => x.Name != null ? x.Name.ElementAt(0) : '\0').GroupBy(x => x.Name != null ? x.Name.ToLower().ElementAt(0): '\0');
            });
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => IsMusicLibraryEmpty = false);
            if (!Artists.Any())
            {
                await StartIndexing();
                return;
            }
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                LoadingState = LoadingState.Loaded;
                IsLoaded = true;
                IsBusy = false;
            });
            MusicCollectionLoaded(null, "music");
            // Routine check to add new files if there are new ones
            await MusicLibraryManagement.GetAllMusicFolders(true);
#if WINDOWS_PHONE_APP
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (App.ApplicationFrame != null)
                    StatusBarHelper.SetDefaultForPage(App.ApplicationFrame.SourcePageType);
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

            await DispatchHelper.InvokeAsync(async () =>
            {
                CurrentIndexingStatus = "Searching for music";
                IsBusy = true;
                IsLoaded = false;
                OnPropertyChanged("IsBusy");
                OnPropertyChanged("IsLoaded");
#if WINDOWS_PHONE_APP
                StatusBarHelper.UpdateTitle("Searching for music ...");
#endif
            });
            _artistDataRepository = new ArtistDataRepository();
            _artistDataRepository.Initialize();
            _trackDataRepository.Initialize();
            _albumDataRepository.Initialize();

            await MusicLibraryManagement.GetAllMusicFolders();

            await LoadFromDatabase();

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
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
                if (App.ApplicationFrame != null)
                    StatusBarHelper.SetDefaultForPage(App.ApplicationFrame.SourcePageType);
#endif
            });
            await GetFavoriteAndRandomAlbums();
        }


        private async Task LoadFromDatabase()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Artists.Clear();
                Tracks.Clear();
            });
            await MusicLibraryManagement.LoadFromSQL();
            await DispatchHelper.InvokeAsync(() =>
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
