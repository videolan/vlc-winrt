﻿/**********************************************************************
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
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Model.Search;

namespace VLC_WINRT_APP.ViewModels.MusicVM
{
    public class MusicLibraryVM : BindableBase
    {
        public ArtistDataRepository _artistDataRepository = new ArtistDataRepository();
        public TrackDataRepository _trackDataRepository = new TrackDataRepository();
        public AlbumDataRepository _albumDataRepository = new AlbumDataRepository();
        public TracklistItemRepository TracklistItemRepository = new TracklistItemRepository();
        public TrackCollectionRepository TrackCollectionRepository = new TrackCollectionRepository();
        public delegate void LoadingEnded(object sender, string myValue);
        public static LoadingEnded MusicCollectionLoaded = delegate { };
        #region private fields
        private ObservableCollection<SearchResult> _searchResults = new ObservableCollection<SearchResult>();
        private ObservableCollection<ArtistItem> _artistses = new ObservableCollection<ArtistItem>();
        private ObservableCollection<TrackItem> _tracks = new ObservableCollection<TrackItem>();
        private ObservableCollection<AlbumItem> _favoriteAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _randomAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _albums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<TrackCollection> _trackCollections = new ObservableCollection<TrackCollection>();
        private IEnumerable<IGrouping<char, TrackItem>> _alphaGroupedTracks;
        #endregion
        #region private props
        private SidebarState _sidebarState;
        private LoadingState _loadingState;
        private StartMusicIndexingCommand _startMusicIndexingCommand;
        private AddToPlaylistCommand _addToPlaylistCommand;
        private TrackCollectionClickedCommand _trackCollectionClickedCommand;
        private ShowCreateNewPlaylistPane _showCreateNewPlaylistPaneCommamd;
        private ArtistAlbumsSemanticZoomInvertZoomCommand _artistAlbumsSemanticZoomInvertZoomCommand;
        private ChangeAlbumArtCommand _changeAlbumArtCommand;
        private DownloadAlbumArtCommand _downloadAlbumArtCommand;
        private AlbumClickedCommand _albumClickedCommand;
        private ArtistClickedCommand _artistClickedCommand;
        private TrackClickedCommand _trackClickedCommand;
        private PlayArtistAlbumsCommand _playArtistAlbumsCommand;
        private PlayAllRandomCommand _playAllRandomCommand;
        private PlayAllSongsCommand _playAllSongsCommand;
        private OpenAddAlbumToPlaylistDialog _openAddAlbumToPlaylistDialogCommand;
        private BingLocationShowCommand _bingLocationShowCommand;
        private DeletePlaylistCommand _deletePlaylistCommand;
        private DeleteSelectedTracksInPlaylistCommand _deleteSelectedTracksInPlaylistCommand;

        private AlbumItem _currentAlbum;
        private ArtistItem _currentArtist;
        private TrackCollection _currentTrackCollection;
        private bool _isLoaded = false;
        private bool _isBusy = false;
        private bool _isMusicLibraryEmpty = true;
        private bool _isAlbumPageShown = false;
        private bool _isMainPageMusicArtistAlbumsSemanticZoomViewedIn;
        public MusicView _musicView;
        private string _searchTag;
        #endregion

        #region public fields
        public ObservableCollection<SearchResult> SearchResults
        {
            get { return _searchResults; }
            set { SetProperty(ref _searchResults, value); }
        }

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

        public ObservableCollection<ArtistItem> Artists
        {
            get { return _artistses; }
            set { SetProperty(ref _artistses, value); }
        }

        public ObservableCollection<TrackItem> Tracks
        {
            get { return _tracks; }
            set
            {
                SetProperty(ref _tracks, value);
            }
        }

        public ObservableCollection<AlbumItem> Albums
        {
            get { return _albums; }
            set { SetProperty(ref _albums, value); }
        }

        #endregion
        #region public props
        public MusicView MusicView
        {
            get { return _musicView; }
            set { SetProperty(ref _musicView, value); }
        }


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
        public string SearchTag
        {
            get { return _searchTag; }
            set
            {
                if (string.IsNullOrEmpty(_searchTag) && !string.IsNullOrEmpty(value))
                    Locator.MainVM.ChangeMainPageMusicViewCommand.Execute(4);
                if (!string.IsNullOrEmpty(value))
                    SearchHelpers.SearchMusic(value, SearchResults);
                SetProperty(ref _searchTag, value);
            }
        }

        public StartMusicIndexingCommand StartMusicIndexingCommand
        {
            get { return _startMusicIndexingCommand ?? (_startMusicIndexingCommand = new StartMusicIndexingCommand()); }
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

        public PlayArtistAlbumsCommand PlayArtistAlbumsCommand
        {
            get { return _playArtistAlbumsCommand ?? (_playArtistAlbumsCommand = new PlayArtistAlbumsCommand()); }
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

        public PlayAllSongsCommand PlayAllSongsCommand
        {
            get { return _playAllSongsCommand ?? (_playAllSongsCommand = new PlayAllSongsCommand()); }
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

        public BingLocationShowCommand BingLocationShowCommand { get { return _bingLocationShowCommand ?? (_bingLocationShowCommand = new BingLocationShowCommand()); } }

        public DeletePlaylistCommand DeletePlaylistCommand
        {
            get { return _deletePlaylistCommand ?? (_deletePlaylistCommand = new DeletePlaylistCommand()); }
        }

        public DeleteSelectedTracksInPlaylistCommand DeleteSelectedTracksInPlaylistCommand
        {
            get { return _deleteSelectedTracksInPlaylistCommand ?? (_deleteSelectedTracksInPlaylistCommand = new DeleteSelectedTracksInPlaylistCommand()); }
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
        public MusicLibraryVM()
        {
            LoadingState = LoadingState.NotLoaded;
            _albumClickedCommand = new AlbumClickedCommand();
            _artistClickedCommand = new ArtistClickedCommand();
            _trackClickedCommand = new TrackClickedCommand();
            _changeAlbumArtCommand = new ChangeAlbumArtCommand();
            _downloadAlbumArtCommand = new DownloadAlbumArtCommand();
            _artistAlbumsSemanticZoomInvertZoomCommand = new ArtistAlbumsSemanticZoomInvertZoomCommand();
            _playAllRandomCommand = new PlayAllRandomCommand();
        }

        public async Task Initialize()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Locator.MainVM.InformationText = "Loading music";
                LoadingState = LoadingState.Loading;
            });
            await GetFavoriteAndRandomAlbums();
            await GetMusicFromLibrary();
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
                Locator.MainVM.InformationText = "";
            });
            MusicCollectionLoaded(null, "music");
            await PerformRoutineCheckIfNotBusy();
#if WINDOWS_PHONE_APP
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (App.ApplicationFrame != null)
                    StatusBarHelper.SetDefaultForPage(App.ApplicationFrame.SourcePageType);
            });
#endif
        }

        public async Task PerformRoutineCheckIfNotBusy()
        {
            // Routine check to add new files if there are new ones
            if (!IsBusy)
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    IsBusy = true;
                });
                await MusicLibraryManagement.DoRoutineMusicLibraryCheck();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
                {
                    IsBusy = false;
                });
            }
        }

        public async Task StartIndexing()
        {
            _artistDataRepository.Drop();
            _trackDataRepository.Drop();
            _albumDataRepository.Drop();

            await DispatchHelper.InvokeAsync(() =>
            {
                Locator.MainVM.InformationText = "Searching for music";
                IsBusy = true;
                IsLoaded = false;
                OnPropertyChanged("IsBusy");
                OnPropertyChanged("IsLoaded");
            });
            _artistDataRepository = new ArtistDataRepository();
            _artistDataRepository.Initialize();
            _trackDataRepository.Initialize();
            _albumDataRepository.Initialize();

            await MusicLibraryManagement.DoRoutineMusicLibraryCheck();
            await LoadFromDatabase();

            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
                Locator.MainVM.InformationText = "";
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

        #endregion
    }
}
