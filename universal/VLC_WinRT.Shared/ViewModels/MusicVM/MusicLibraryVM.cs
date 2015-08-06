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
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using VLC_WinRT.Database;
using VLC_WinRT.Helpers;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Commands.MusicLibrary;
using VLC_WinRT.Model.Search;
using VLC_WinRT.Utils;
using VLC_WinRT.Commands.MusicPlayer;

namespace VLC_WinRT.ViewModels.MusicVM
{
    public class MusicLibraryVM : BindableBase
    {
        #region databases
        public ArtistDatabase _artistDatabase = new ArtistDatabase();
        public TrackDatabase _trackDatabase = new TrackDatabase();
        public AlbumDatabase _albumDatabase = new AlbumDatabase();
        public TracklistItemRepository TracklistItemRepository = new TracklistItemRepository();
        public TrackCollectionRepository TrackCollectionRepository = new TrackCollectionRepository();
        #endregion
        #region task completion sources
        public TaskCompletionSource<bool> MusicCollectionLoaded = new TaskCompletionSource<bool>();
        #endregion
        #region private fields
        private ObservableCollection<ArtistItem> _artists = new ObservableCollection<ArtistItem>();
        private ObservableCollection<ArtistItem> _topArtists = new ObservableCollection<ArtistItem>(); 

        private ObservableCollection<TrackItem> _tracks = new ObservableCollection<TrackItem>();
        private ObservableCollection<TrackCollection> _trackCollections = new ObservableCollection<TrackCollection>();

        private ObservableCollection<AlbumItem> _albums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _favoriteAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _randomAlbums = new ObservableCollection<AlbumItem>();

        private IEnumerable<IGrouping<char, TrackItem>> _groupedTracks;
        private ObservableCollection<GroupItemList<AlbumItem>> _groupedAlbums;
        private IEnumerable<IGrouping<string, ArtistItem>> _groupedArtists;
        #endregion
        #region private props
        private LoadingState _loadingState = LoadingState.NotLoaded;

        private AlbumItem _currentAlbum;
        private ArtistItem _currentArtist;
        private TrackCollection _currentTrackCollection;
        private bool _isLoaded = false;
        private bool _isBusy = false;
        private bool _isMusicLibraryEmpty = true;
        public MusicView _musicView;
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

        public ObservableCollection<ArtistItem> Artists
        {
            get { return _artists; }
            set { SetProperty(ref _artists, value); }
        }
        public ObservableCollection<ArtistItem> TopArtists
        {
            get { return _topArtists; }
            set { SetProperty(ref _topArtists, value); }
        }

        public IEnumerable<IGrouping<string, ArtistItem>> GroupedArtists
        {
            get { return _groupedArtists; }
            set { SetProperty(ref _groupedArtists, value); }
        }

        public ObservableCollection<TrackItem> Tracks
        {
            get { return _tracks; }
            set { SetProperty(ref _tracks, value); }
        }

        public IEnumerable<IGrouping<char, TrackItem>> GroupedTracks
        {
            get { return _groupedTracks; }
            set { SetProperty(ref _groupedTracks, value); }
        }

        public ObservableCollection<AlbumItem> Albums
        {
            get { return _albums; }
            set { SetProperty(ref _albums, value); }
        }
        

        public ObservableCollection<GroupItemList<AlbumItem>> GroupedAlbums
        {
            get { return _groupedAlbums; }
            set { SetProperty(ref _groupedAlbums, value); }
        }
        #endregion
        #region public props
        public MusicView MusicView
        {
            get { return _musicView; }
            set { SetProperty(ref _musicView, value); }
        }
        
        public LoadingState LoadingState
        {
            get { return _loadingState; }
            set { SetProperty(ref _loadingState, value); }
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
        
        public StartMusicIndexingCommand StartMusicIndexingCommand { get; } = new StartMusicIndexingCommand();

        public AddToPlaylistCommand AddToPlaylistCommand { get; } = new AddToPlaylistCommand();

        public TrackCollectionClickedCommand TrackCollectionClickedCommand { get; } = new TrackCollectionClickedCommand();

        public ShowCreateNewPlaylistPane ShowCreateNewPlaylistPaneCommand { get; } = new ShowCreateNewPlaylistPane();
        
        public ChangeAlbumArtCommand ChangeAlbumArtCommand { get; } = new ChangeAlbumArtCommand();

        public DownloadAlbumArtCommand DownloadAlbumArtCommand { get; } = new DownloadAlbumArtCommand();

        public AlbumClickedCommand AlbumClickedCommand { get; } = new AlbumClickedCommand();

        public ArtistClickedCommand ArtistClickedCommand { get; } = new ArtistClickedCommand();

        public ResetCurrentArtistAndAlbumCommand ResetCurrentArtistAndAlbumCommand { get;} = new ResetCurrentArtistAndAlbumCommand();

        public PlayArtistAlbumsCommand PlayArtistAlbumsCommand { get; } = new PlayArtistAlbumsCommand();

        public PlayAlbumCommand PlayAlbumCommand { get; } = new PlayAlbumCommand();

        public TrackClickedCommand TrackClickedCommand { get; } = new TrackClickedCommand();

        public PlayAllRandomCommand PlayAllRandomCommand { get; } = new PlayAllRandomCommand();

        public PlayAllSongsCommand PlayAllSongsCommand { get; } = new PlayAllSongsCommand();

        public OpenAddAlbumToPlaylistDialog OpenAddAlbumToPlaylistDialogCommand { get; } = new OpenAddAlbumToPlaylistDialog();

        public BingLocationShowCommand BingLocationShowCommand { get; } = new BingLocationShowCommand();

        public DeletePlaylistCommand DeletePlaylistCommand { get; } = new DeletePlaylistCommand();
        public DeletePlaylistTrackCommand DeletePlaylistTrackCommand { get; } = new DeletePlaylistTrackCommand();

        public DeleteSelectedTracksInPlaylistCommand DeleteSelectedTracksInPlaylistCommand { get; } = new DeleteSelectedTracksInPlaylistCommand();

        public ArtistItem CurrentArtist
        {
            get { return _currentArtist; }
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
                Debug.WriteLine("Starting music indexation");
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
            MusicCollectionLoaded.SetResult(true);
            await PerformRoutineCheckIfNotBusy();
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
                    Locator.MainVM.InformationText = "";
                });
            }
        }

        public async Task StartIndexing()
        {
            // Clean DBs
            _artistDatabase.Drop();
            _trackDatabase.Drop();
            _albumDatabase.Drop();
            
            await DispatchHelper.InvokeAsync(() =>
            {            
                // Clear runtime collections
                _albums?.Clear();
                _artists?.Clear();
                _tracks?.Clear();
                _randomAlbums?.Clear();
                _favoriteAlbums?.Clear();
                _groupedAlbums?.Clear();
                Locator.MainVM.InformationText = "Searching for music";
                IsBusy = true;
                IsLoaded = false;
                OnPropertyChanged("IsBusy");
                OnPropertyChanged("IsLoaded");
            });
            _artistDatabase = new ArtistDatabase();
            _artistDatabase.Initialize();
            _trackDatabase.Initialize();
            _albumDatabase.Initialize();

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
        #endregion
    }
}
