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
        public MusicLibraryManagement MusicLibrary = new MusicLibraryManagement();
        #region private fields
        private ObservableCollection<ArtistItem> _topArtists = new ObservableCollection<ArtistItem>();
        private ObservableCollection<ArtistItem> _recommendedArtists = new ObservableCollection<ArtistItem>(); // recommanded with MusicFlow

        private ObservableCollection<TrackCollection> _trackCollections = new ObservableCollection<TrackCollection>();

        private ObservableCollection<AlbumItem> _favoriteAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<AlbumItem> _randomAlbums = new ObservableCollection<AlbumItem>();

        #endregion
        #region private props
        private LoadingState _loadingStateArtists = LoadingState.NotLoaded;
        private LoadingState _loadingStateAlbums = LoadingState.NotLoaded;
        private LoadingState _loadingStateTracks = LoadingState.NotLoaded;
        private LoadingState _loadingStatePlaylists = LoadingState.NotLoaded;

        private ArtistItem _focusOnAnArtist; // recommended with MusicFlow
        private TrackItem _currentTrack;
        private AlbumItem _currentAlbum;
        private ArtistItem _currentArtist;
        private TrackCollection _currentTrackCollection;
        private bool _isLoaded = false;
        private bool _isBusy = false;
        public MusicView _musicView;
        #endregion

        #region public fields

        public ObservableCollection<TrackCollection> TrackCollections
        {
            get { return MusicLibrary.TrackCollections; }
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

        public ObservableCollection<ArtistItem> TopArtists
        {
            get { return _topArtists; }
            set { SetProperty(ref _topArtists, value); }
        }

        public ObservableCollection<ArtistItem> RecommendedArtists
        {
            get { return _recommendedArtists; }
            set { SetProperty(ref _recommendedArtists, value); }
        }

        public IEnumerable<IGrouping<string, ArtistItem>> GroupedArtists
        {
            get { return MusicLibrary.OrderArtists(); }
        }

        public IEnumerable<IGrouping<char, TrackItem>> GroupedTracks
        {
            get { return MusicLibrary.OrderTracks(); }
        }

        public ObservableCollection<GroupItemList<AlbumItem>> GroupedAlbums
        {
            get { return MusicLibrary.OrderAlbums(Locator.SettingsVM.AlbumsOrderType, Locator.SettingsVM.AlbumsOrderListing); }
        }
        #endregion
        #region public props
        public MusicView MusicView
        {
            get { return _musicView; }
            set { SetProperty(ref _musicView, value); }
        }

        public LoadingState LoadingStateArtists
        {
            get { return _loadingStateArtists; }
            set { SetProperty(ref _loadingStateArtists, value); }
        }
        public LoadingState LoadingStateAlbums
        {
            get { return _loadingStateAlbums; }
            set { SetProperty(ref _loadingStateAlbums, value); }
        }
        public LoadingState LoadingStateTracks
        {
            get { return _loadingStateTracks; }
            set { SetProperty(ref _loadingStateTracks, value); }
        }
        public LoadingState LoadingStatePlaylists
        {
            get { return _loadingStatePlaylists; }
            set { SetProperty(ref _loadingStatePlaylists, value); }
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

        public bool IsMusicLibraryEmpty => MusicLibrary.Artists?.Count == 0 && MusicLibrary.Albums?.Count == 0 && MusicLibrary.Tracks?.Count == 0;

        public StartMusicIndexingCommand StartMusicIndexingCommand { get; } = new StartMusicIndexingCommand();

        public AddToPlaylistCommand AddToPlaylistCommand { get; } = new AddToPlaylistCommand();

        public TrackCollectionClickedCommand TrackCollectionClickedCommand { get; } = new TrackCollectionClickedCommand();

        public ShowCreateNewPlaylistPane ShowCreateNewPlaylistPaneCommand { get; } = new ShowCreateNewPlaylistPane();
        
        public ChangeAlbumArtCommand ChangeAlbumArtCommand { get; } = new ChangeAlbumArtCommand();

        public DownloadAlbumArtCommand DownloadAlbumArtCommand { get; } = new DownloadAlbumArtCommand();

        public AlbumClickedCommand AlbumClickedCommand { get; } = new AlbumClickedCommand();

        public ArtistClickedCommand ArtistClickedCommand { get; } = new ArtistClickedCommand();

        public ResetCurrentArtistAndAlbumCommand ResetCurrentArtistAndAlbumCommand { get; } = new ResetCurrentArtistAndAlbumCommand();

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

        public StartTrackMetaEdit StartTrackMetaEdit { get; } = new StartTrackMetaEdit();

        public ArtistItem FocusOnAnArtist // Music Flow recommandation
        {
            get { return _focusOnAnArtist; }
            set { SetProperty(ref _focusOnAnArtist, value); }
        }

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

        public TrackItem CurrentTrack
        {
            get { return _currentTrack; }
            set { SetProperty(ref _currentTrack, value); }
        }

        public TrackCollection CurrentTrackCollection
        {
            get { return _currentTrackCollection; }
            set { SetProperty(ref _currentTrackCollection, value); }
        }

        #endregion

        public void ResetLibrary()
        {
            LoadingStateAlbums = LoadingState.NotLoaded;
            LoadingStateArtists = LoadingState.NotLoaded;
            LoadingStateTracks = LoadingState.NotLoaded;
            LoadingStatePlaylists = LoadingState.NotLoaded;

            RecommendedArtists?.Clear();
            RecommendedArtists = new ObservableCollection<ArtistItem>();

            RandomAlbums?.Clear();
            RandomAlbums = new ObservableCollection<AlbumItem>();
            FavoriteAlbums?.Clear();
            FavoriteAlbums = new ObservableCollection<AlbumItem>();

            GC.Collect();
        }

        public void OnNavigatedTo()
        {
            ResetLibrary();
            Task.Run(() => MusicLibrary.Initialize());
        }

        public void OnNavigatedToArtists()
        {
            if (LoadingStateArtists == LoadingState.NotLoaded)
            {
                InitializeArtists();
            }
        }

        public void OnNavigatedToAlbums()
        {
            if (LoadingStateAlbums == LoadingState.NotLoaded)
            {
                InitializeAlbums();
            }
        }

        public void OnNavigatedToTracks()
        {
            if (LoadingStateTracks == LoadingState.NotLoaded)
            {
                InitializeTracks();
            }
        }

        public void OnNavigatedToPlaylists()
        {
            if (LoadingStatePlaylists == LoadingState.NotLoaded)
            {
                InitializePlaylists();
            }
        }

        public void OnNavigatedFrom()
        {
            ResetLibrary();
        }

        public void OnNavigatedFromArtists()
        {
            if (MusicLibrary.Artists != null)
                MusicLibrary.Artists.CollectionChanged -= Artists_CollectionChanged;
        }

        public void OnNavigatedFromAlbums()
        {
            if (MusicLibrary.Albums != null)
                MusicLibrary.Albums.CollectionChanged -= Albums_CollectionChanged;
        }

        Task InitializeAlbums()
        {
            return Task.Run(async () =>
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.LoadingMusic;
                    LoadingStateAlbums = LoadingState.Loading;
                });

                if (MusicLibrary.Albums != null)
                    MusicLibrary.Albums.CollectionChanged += Albums_CollectionChanged;
                await MusicLibrary.LoadAlbumsFromDatabase();
                var randomAlbums = await MusicLibrary.LoadRandomAlbumsFromDatabase();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    RandomAlbums = randomAlbums;

                    Locator.MainVM.InformationText = String.Empty;
                    LoadingStateAlbums = LoadingState.Loaded;
                    OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                });
            });
        }

        private async void Albums_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await OrderAlbums();
        }

        public async Task OrderAlbums()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(GroupedAlbums));
            });
        }

        Task InitializeArtists()
        {
            return Task.Run(async () =>
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.LoadingMusic;
                    LoadingStateArtists = LoadingState.Loading;
                });

                if (MusicLibrary.Artists != null)
                    MusicLibrary.Artists.CollectionChanged += Artists_CollectionChanged;
                await MusicLibrary.LoadArtistsFromDatabase();
                var recommendedArtists = await MusicLibrary.LoadRandomArtistsFromDatabase();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    RecommendedArtists = recommendedArtists;

                    Locator.MainVM.InformationText = String.Empty;
                    LoadingStateArtists = LoadingState.Loaded;
                    OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                });
            });
        }

        private async void Artists_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await OrderArtists();
        }

        async Task OrderArtists()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(GroupedArtists));
            });
        }

        Task InitializeTracks()
        {
            return Task.Run(async () =>
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.LoadingMusic;
                    LoadingStateTracks = LoadingState.Loading;
                });

                await MusicLibrary.LoadTracksFromDatabase();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = String.Empty;
                    LoadingStateTracks = LoadingState.Loaded;
                    OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                });
                await OrderTracks();
            });
        }

        async Task OrderTracks()
        {
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(GroupedTracks));
            });
        }

        Task InitializePlaylists()
        {
            return Task.Run(async () =>
            {
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.LoadingMusic;
                    LoadingStatePlaylists = LoadingState.Loading;
                });

                await MusicLibrary.LoadPlaylistsFromDatabase();
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    OnPropertyChanged(nameof(TrackCollections));

                    Locator.MainVM.InformationText = String.Empty;
                    LoadingStatePlaylists = LoadingState.Loaded;
                });
            });
        }
        #region methods            

        static async Task InsertIntoGroupAlbum(AlbumItem album)
        {
            if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByArtist)
            {
                var artist = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => x.Key == Strings.HumanizedArtistName(album.Artist));
                if (artist == null)
                {
                    artist = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedArtistName(album.Artist) };
                    int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x => string.Compare(x.Key, artist.Key) < 0));
                    i++;
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, artist));
                }
                else await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => artist.Add(album));
            }
            else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByDate)
            {
                var year = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => x.Key == Strings.HumanizedYear(album.Year));
                if (year == null)
                {
                    var newyear = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedYear(album.Year) };
                    int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x => string.Compare(x.Key, newyear.Key) < 0));
                    i++;
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, newyear));
                }
                else await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => year.Add(album));
            }
            else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByAlbum)
            {
                var firstChar = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => x.Key == Strings.HumanizedAlbumFirstLetter(album.Name));
                if (firstChar == null)
                {
                    var newChar = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedAlbumFirstLetter(album.Name) };
                    int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x => string.Compare(x.Key, newChar.Key) < 0));
                    i++;
                    await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, newChar));
                }
                else await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () => firstChar.Add(album));
            }
        }
        #endregion
    }
}
