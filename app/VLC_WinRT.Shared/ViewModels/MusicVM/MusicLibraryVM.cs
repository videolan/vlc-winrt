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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
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
using Windows.UI.Xaml;

namespace VLC_WinRT.ViewModels.MusicVM
{
    public class MusicLibraryVM : BindableBase
    {
        public MusicLibraryManagement MusicLibrary = new MusicLibraryManagement();
        #region private fields
        private ObservableCollection<GroupItemList<ArtistItem>> _groupedArtists;
        private ObservableCollection<ArtistItem> _topArtists = new ObservableCollection<ArtistItem>();
        private ObservableCollection<ArtistItem> _recommendedArtists = new ObservableCollection<ArtistItem>(); // recommanded with MusicFlow

        private ObservableCollection<TrackCollection> _trackCollections = new ObservableCollection<TrackCollection>();

        private ObservableCollection<GroupItemList<AlbumItem>> _groupedAlbums;
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
        public List<MusicView> MusicViewCollection { get; set; } = new List<MusicView>()
        {
            MusicView.Artists,
            MusicView.Albums,
            MusicView.Songs,
            MusicView.Playlists
        };

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

        public ObservableCollection<GroupItemList<ArtistItem>> GroupedArtists
        {
            get { return _groupedArtists; }
            set { SetProperty(ref _groupedArtists, value); }
        }

        public IEnumerable<IGrouping<char, TrackItem>> GroupedTracks
        {
            get { return MusicLibrary.OrderTracks(); }
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
            get
            {
                var musicView = ApplicationSettingsHelper.ReadSettingsValue(nameof(MusicView), false);
                if (musicView == null)
                {
                    _musicView = MusicView.Artists;
                }
                else
                {
                    _musicView = (MusicView)musicView;
                }
                return _musicView;
            }
            set
            {
                ApplicationSettingsHelper.SaveSettingsValue(nameof(MusicView), (int)value, false);
                SetProperty(ref _musicView, value);
                OnPropertyChanged(nameof(AlbumsCollectionsButtonVisible));
                OnPropertyChanged(nameof(ShuffleButtonVisible));
                OnPropertyChanged(nameof(PlaylistButtonsVisible));
            }
        }

        public Visibility AlbumsCollectionsButtonVisible { get { return MusicView == MusicView.Albums ? Visibility.Visible : Visibility.Collapsed; } }
        public Visibility ShuffleButtonVisible
        {
            get
            {
                return (MusicView == MusicView.Albums || MusicView == MusicView.Artists || MusicView == MusicView.Songs) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility PlaylistButtonsVisible
        {
            get
            {
                return MusicView == MusicView.Playlists ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility MusicLibraryEmptyVisible => IsMusicLibraryEmpty ? Visibility.Visible : Visibility.Collapsed;

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

        public AlbumTrackClickedCommand AlbumTrackClickedCommand { get; } = new AlbumTrackClickedCommand();

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
                OnPropertyChanged(nameof(CurrentArtist));
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
            if (LoadingStateArtists == LoadingState.NotLoaded && GroupedArtists == null)
            {
                InitializeArtists();
            }
            else
            {
                OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
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

        public Task OnNavigatedFromArtists()
        {
            if (MusicLibrary.Artists != null)
            {
                MusicLibrary.Artists.CollectionChanged -= Artists_CollectionChanged;
                MusicLibrary.Artists.Clear();
            }

            return DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                GroupedArtists = null;
                LoadingStateArtists = LoadingState.NotLoaded;
                CurrentArtist = null;
            });
        }

        public Task OnNavigatedFromAlbums()
        {
            if (MusicLibrary.Albums != null)
            {
                MusicLibrary.Albums.CollectionChanged -= Albums_CollectionChanged;
                MusicLibrary.Albums.Clear();
                return DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    GroupedAlbums = null;
                    LoadingStateAlbums = LoadingState.NotLoaded;
                });
            }
            return null;
        }

        Task InitializeAlbums()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.LoadingMusic;
                    LoadingStateAlbums = LoadingState.Loading;
                    GroupedAlbums = new ObservableCollection<GroupItemList<AlbumItem>>();
                });

                if (MusicLibrary.Albums != null)
                    MusicLibrary.Albums.CollectionChanged += Albums_CollectionChanged;
                await MusicLibrary.LoadAlbumsFromDatabase();
                var randomAlbums = await MusicLibrary.LoadRandomAlbumsFromDatabase();
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    RandomAlbums = randomAlbums;

                    Locator.MainVM.InformationText = String.Empty;
                    LoadingStateAlbums = LoadingState.Loaded;
                    OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                    OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
                });
            });
        }

        private async void Albums_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MusicLibrary.Albums?.Count == 0 || MusicLibrary.Albums?.Count == 1)
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                    OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
                });
            }

            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.Count > 0)
            {
                foreach (var newItem in e.NewItems)
                {
                    var album = (AlbumItem)newItem;
                    await InsertIntoGroupAlbum(album);
                }
            }
            else
                await OrderAlbums();
        }

        public async Task OrderAlbums()
        {
            _groupedAlbums = MusicLibrary.OrderAlbums(Locator.SettingsVM.AlbumsOrderType, Locator.SettingsVM.AlbumsOrderListing);
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(GroupedAlbums));
            });
        }

        Task InitializeArtists()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.LoadingMusic;
                    LoadingStateArtists = LoadingState.Loading;
                    GroupedArtists = new ObservableCollection<GroupItemList<ArtistItem>>();
                });

                if (MusicLibrary.Artists != null)
                    MusicLibrary.Artists.CollectionChanged += Artists_CollectionChanged;
                await MusicLibrary.LoadArtistsFromDatabase();
                var recommendedArtists = await MusicLibrary.LoadRandomArtistsFromDatabase();
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    RecommendedArtists = recommendedArtists;

                    Locator.MainVM.InformationText = String.Empty;
                    LoadingStateArtists = LoadingState.Loaded;
                    OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                    OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
                });
            });
        }

        private async void Artists_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (MusicLibrary.Artists?.Count == 0 || MusicLibrary.Artists?.Count == 1)
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                    OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
                });
            }

            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.Count > 0)
            {
                foreach (var newItem in e.NewItems)
                {
                    var artist = (ArtistItem)newItem;
                    await InsertIntoGroupArtist(artist);
                }
            }
            else
                await OrderArtists();
        }

        async Task OrderArtists()
        {
            _groupedArtists = MusicLibrary.OrderArtists();
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(GroupedArtists));
            });
        }

        Task InitializeTracks()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.LoadingMusic;
                    LoadingStateTracks = LoadingState.Loading;
                });

                await MusicLibrary.LoadTracksFromDatabase();
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = String.Empty;
                    LoadingStateTracks = LoadingState.Loaded;
                    OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                    OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
                });
                await OrderTracks();
            });
        }

        async Task OrderTracks()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(GroupedTracks));
            });
        }

        Task InitializePlaylists()
        {
            return Task.Run(async () =>
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Locator.MainVM.InformationText = Strings.LoadingMusic;
                    LoadingStatePlaylists = LoadingState.Loading;
                });

                await MusicLibrary.LoadPlaylistsFromDatabase();
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
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
            try
            {
                if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByArtist)
                {
                    var artist = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => (string)x.Key == Strings.HumanizedArtistName(album.Artist));
                    if (artist == null)
                    {
                        artist = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedArtistName(album.Artist) };
                        int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x =>
                        string.Compare((string)x.Key, (string)artist.Key, StringComparison.OrdinalIgnoreCase) < 0));
                        i++;
                        await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, artist));
                    }
                    else await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => artist.Add(album));
                }
                else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByDate)
                {
                    var year = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => (string)x.Key == Strings.HumanizedYear(album.Year));
                    if (year == null)
                    {
                        var newyear = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedYear(album.Year) };
                        int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x => string.Compare((string)x.Key, (string)newyear.Key) < 0));
                        i++;
                        await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, newyear));
                    }
                    else await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => year.Add(album));
                }
                else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByAlbum)
                {
                    var firstChar = Locator.MusicLibraryVM.GroupedAlbums.FirstOrDefault(x => (string)x.Key == Strings.HumanizedAlbumFirstLetter(album.Name));
                    if (firstChar == null)
                    {
                        var newChar = new GroupItemList<AlbumItem>(album) { Key = Strings.HumanizedAlbumFirstLetter(album.Name) };
                        int i = Locator.MusicLibraryVM.GroupedAlbums.IndexOf(Locator.MusicLibraryVM.GroupedAlbums.LastOrDefault(x => string.Compare((string)x.Key, (string)newChar.Key) < 0));
                        i++;
                        await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => Locator.MusicLibraryVM.GroupedAlbums.Insert(i, newChar));
                    }
                    else await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => firstChar.Add(album));
                }
            }
            catch { }
        }

        async Task InsertIntoGroupArtist(ArtistItem artist)
        {
            try
            {
                var supposedFirstChar = Strings.HumanizedArtistFirstLetter(artist.Name);
                var firstChar = GroupedArtists.FirstOrDefault(x => (string)x.Key == supposedFirstChar);
                if (firstChar == null)
                {
                    var newChar = new GroupItemList<ArtistItem>(artist)
                    {
                        Key = supposedFirstChar
                    };
                    if (GroupedArtists == null) return;
                    int i = GroupedArtists.IndexOf(Locator.MusicLibraryVM.GroupedArtists.LastOrDefault(x => string.Compare((string)x.Key, (string)newChar.Key) < 0));
                    i++;
                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.High, () => GroupedArtists.Insert(i, newChar));
                }
                else
                {
                    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.High, () => firstChar.Add(artist));
                }
            }
            catch { }
        }
        #endregion
    }
}
