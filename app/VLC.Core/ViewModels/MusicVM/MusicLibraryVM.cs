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
using System.Threading.Tasks;
using Windows.UI.Core;
using VLC.Helpers;
using VLC.Model;
using VLC.Model.Music;
using VLC.Commands.MusicLibrary;
using VLC.Utils;
using VLC.Commands.MusicPlayer;
using Windows.UI.Xaml;
using VLC.Commands.MediaLibrary;
using VLC.Commands.Navigation;

namespace VLC.ViewModels.MusicVM
{
    public class MusicLibraryVM : BindableBase
    {
        public MusicLibraryVM()
        {
            
        }
        #region private fields
        private ObservableCollection<GroupItemList<TrackItem>> _groupedTracks;

        private ObservableCollection<GroupItemList<ArtistItem>> _groupedArtists;
        private ObservableCollection<ArtistItem> _topArtists = new ObservableCollection<ArtistItem>();

        private ObservableCollection<PlaylistItem> _trackCollections = new ObservableCollection<PlaylistItem>();

        private ObservableCollection<AlbumItem> _groupedAlbums;
        private List<AlbumItem> _recommendedAlbums = new List<AlbumItem>();

        #endregion
        #region private props
        private LoadingState _loadingStateArtists = LoadingState.NotLoaded;
        private LoadingState _loadingStateAlbums = LoadingState.NotLoaded;
        private LoadingState _loadingStateTracks = LoadingState.NotLoaded;
        private LoadingState _loadingStatePlaylists = LoadingState.NotLoaded;

        private TrackItem _currentMedia;
        private AlbumItem _currentAlbum;
        private ArtistItem _currentArtist;
        private PlaylistItem _currentMediaCollection;
        private bool _isLoaded = false;
        private MusicView _musicView;
        #endregion

        #region public fields
        public List<MusicView> MusicViewCollection { get; set; } = new List<MusicView>()
        {
            MusicView.Artists,
            MusicView.Albums,
            MusicView.Songs,
            MusicView.Playlists
        };

        public ObservableCollection<PlaylistItem> TrackCollections
        {
            get { return Locator.MediaLibrary.TrackCollections; }
        }
        
        public List<AlbumItem> RecommendedAlbums
        {
            get { return _recommendedAlbums; }
            set { SetProperty(ref _recommendedAlbums, value); }
        }

        public ObservableCollection<GroupItemList<ArtistItem>> GroupedArtists
        {
            get { return _groupedArtists; }
            set { SetProperty(ref _groupedArtists, value); }
        }

        public ObservableCollection<GroupItemList<TrackItem>> GroupedTracks
        {
            get { return _groupedTracks; }
            set { SetProperty(ref _groupedTracks, value); }
        }

        public ObservableCollection<AlbumItem> GroupedAlbums
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
                    _musicView = MusicView.Albums;
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
                OnPropertyChanged(nameof(PlayArtistButtonVisible));
                OnPropertyChanged(nameof(PlayAlbumButtonVisible));
                OnPropertyChanged(nameof(PlaySongButtonVisible));
                OnPropertyChanged(nameof(AddPlaylistButtonVisible));
            }
        }

        public Visibility AlbumsCollectionsButtonVisible { get { return MusicView == MusicView.Albums ? Visibility.Visible : Visibility.Collapsed; } }

        public Visibility PlayArtistButtonVisible {
            get {
                return MusicView == MusicView.Artists && Locator.SettingsVM.MediaCenterMode
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility PlayAlbumButtonVisible
        {
            get
            {
                return MusicView == MusicView.Albums && Locator.SettingsVM.MediaCenterMode
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility PlaySongButtonVisible
        {
            get
            {
                return MusicView == MusicView.Songs && Locator.SettingsVM.MediaCenterMode
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility AddPlaylistButtonVisible
        {
            get
            {
                return MusicView == MusicView.Playlists && Locator.SettingsVM.MediaCenterMode
                    ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool DesktopMode { get { return !Locator.SettingsVM.MediaCenterMode; } }

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

        public bool IsMusicLibraryEmpty => (LoadingStateArtists == LoadingState.Loaded && Locator.MediaLibrary.Artists.Count == 0)
                                        || (LoadingStateAlbums == LoadingState.Loaded && Locator.MediaLibrary.Albums.Count == 0)
                                        || (LoadingStateTracks == LoadingState.Loaded && Locator.MediaLibrary.Tracks.Count == 0);

        public IndexMediaLibraryCommand IndexMediaLibraryCommand { get; private set; } = new IndexMediaLibraryCommand();

        public AddToPlaylistCommand AddToPlaylistCommand { get; private set; } = new AddToPlaylistCommand();

        public TrackCollectionClickedCommand TrackCollectionClickedCommand { get; private set; } = new TrackCollectionClickedCommand();

        public ShowCreateNewPlaylistPane ShowCreateNewPlaylistPaneCommand { get; private set; } = new ShowCreateNewPlaylistPane();

        public ChangeAlbumArtCommand ChangeAlbumArtCommand { get; private set; } = new ChangeAlbumArtCommand();

        public DownloadAlbumArtCommand DownloadAlbumArtCommand { get; private set; } = new DownloadAlbumArtCommand();

        public AlbumClickedCommand AlbumClickedCommand { get; private set; } = new AlbumClickedCommand();

        public ArtistClickedCommand ArtistClickedCommand { get; private set; } = new ArtistClickedCommand();

        public ResetCurrentArtistAndAlbumCommand ResetCurrentArtistAndAlbumCommand { get; private set; } = new ResetCurrentArtistAndAlbumCommand();

        public PlayArtistAlbumsCommand PlayArtistAlbumsCommand { get; private set; } = new PlayArtistAlbumsCommand();

        public PlayAlbumCommand PlayAlbumCommand { get; private set; } = new PlayAlbumCommand();

        public PlayTrackCommand PlayTrackCommand { get; private set; } = new PlayTrackCommand();

        public AlbumTrackClickedCommand AlbumTrackClickedCommand { get; private set; } = new AlbumTrackClickedCommand();

        public PlayAllRandomCommand PlayAllRandomCommand { get; private set; } = new PlayAllRandomCommand();

        public PlayAllSongsCommand PlayAllSongsCommand { get; private set; } = new PlayAllSongsCommand();

        public OpenAddAlbumToPlaylistDialog OpenAddAlbumToPlaylistDialogCommand { get; private set; } = new OpenAddAlbumToPlaylistDialog();

        public BingLocationShowCommand BingLocationShowCommand { get; private set; } = new BingLocationShowCommand();

        public DeletePlaylistCommand DeletePlaylistCommand { get; private set; } = new DeletePlaylistCommand();

        public DeletePlaylistTrackCommand DeletePlaylistTrackCommand { get; private set; } = new DeletePlaylistTrackCommand();

        public DeleteSelectedTracksInPlaylistCommand DeleteSelectedTracksInPlaylistCommand { get; private set; } = new DeleteSelectedTracksInPlaylistCommand();

        public StartTrackMetaEdit StartTrackMetaEdit { get; private set; } = new StartTrackMetaEdit();

        public SetAlbumViewOrderCommand SetAlbumViewOrder { get; private set; } = new SetAlbumViewOrderCommand();
        public DeleteFromLibraryCommand DeleteFromLibraryCommand { get; private set; } = new DeleteFromLibraryCommand();
        public ChangeMusicViewCommand ChangeMusicViewCommand { get; private set; } = new ChangeMusicViewCommand();

        public ArtistItem CurrentArtist
        {
            get { return _currentArtist; }
            set
            {
                SetProperty(ref _currentArtist, value);
                OnPropertyChanged(nameof(IsCurrentArtistExist));
                OnPropertyChanged(nameof(DisplayArtistAlbumsPaneInArtistsView));
            }
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
            get { return _currentMedia; }
            set { SetProperty(ref _currentMedia, value); }
        }

        public PlaylistItem CurrentTrackCollection
        {
            get { return _currentMediaCollection; }
            set { SetProperty(ref _currentMediaCollection, value); }
        }

        public bool IsCurrentArtistExist => _currentArtist != null;

        public bool DisplayArtistAlbumsPaneInArtistsView => IsCurrentArtistExist;
        #endregion

        public void ResetLibrary()
        {
            LoadingStateAlbums = LoadingState.NotLoaded;
            LoadingStateArtists = LoadingState.NotLoaded;
            LoadingStateTracks = LoadingState.NotLoaded;
            LoadingStatePlaylists = LoadingState.NotLoaded;

            RecommendedAlbums?.Clear();
        }

        public async Task OnNavigatedTo()
        {
            ResetLibrary();
            switch (_musicView)
            {
                case MusicView.Albums:
                    if (LoadingStateAlbums == LoadingState.NotLoaded && GroupedAlbums == null)
                    {
                        await initializeAlbums();
                    }
                    break;
                case MusicView.Artists:
                    if (LoadingStateArtists == LoadingState.NotLoaded && GroupedArtists == null)
                    {
                        initializeArtists();
                    }
                    else
                    {
                        await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                        {
                            OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                            OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
                        });
                    }
                    break;
                case MusicView.Songs:
                    if (LoadingStateTracks == LoadingState.NotLoaded)
                    {
                        await InitializeTracks();
                    }
                    break;
                case MusicView.Playlists:
                    if (LoadingStatePlaylists == LoadingState.NotLoaded)
                    {
                        InitializePlaylists();
                    }
                    break;
                default:
                    break;
            }
        }

        public async Task OnNavigatedFrom()
        {
            ResetLibrary();

            switch (_musicView)
            {
                case MusicView.Albums:
                    if (Locator.MediaLibrary.Albums != null)
                    {
                        Locator.MediaLibrary.Albums.CollectionChanged -= Albums_CollectionChanged;
                        Locator.MediaLibrary.Albums.Clear();
                    }
                    
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                    {
                        GroupedAlbums?.Clear();
                        LoadingStateAlbums = LoadingState.NotLoaded;
                    });

                    break;
                case MusicView.Artists:
                    if (Locator.MediaLibrary.Artists != null)
                    {
                        Locator.MediaLibrary.Artists.CollectionChanged -= Artists_CollectionChanged;
                        Locator.MediaLibrary.Artists.Clear();
                    }

                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                    {
                        GroupedArtists = null;
                        LoadingStateArtists = LoadingState.NotLoaded;
                    });
                    break;
                case MusicView.Songs:
                    if (Locator.MediaLibrary.Tracks != null)
                    {
                        Locator.MediaLibrary.Tracks.CollectionChanged -= Tracks_CollectionChanged;
                        Locator.MediaLibrary.Tracks.Clear();
                    }

                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                    {
                        GroupedTracks = null;
                        LoadingStateTracks = LoadingState.NotLoaded;
                    });
                    break;
                case MusicView.Playlists:
                    break;
                default:
                    break;
            }
        }

        private async Task initializeAlbums()
        {
            LoadingStateAlbums = LoadingState.Loading;
            GroupedAlbums = new ObservableCollection<AlbumItem>();

            Locator.MediaLibrary.Albums.CollectionChanged += Albums_CollectionChanged;
            Locator.MediaLibrary.LoadAlbumsFromDatabase();
            //await RefreshRecommendedAlbums();
            LoadingStateAlbums = LoadingState.Loaded;
            OnPropertyChanged(nameof(IsMusicLibraryEmpty));
            OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
        }

        public async Task RefreshRecommendedAlbums()
        {
            if (MusicView != MusicView.Albums)
                return;
            var recommendedAlbums = Locator.MediaLibrary.LoadRecommendedAlbumsFromDatabase();
            try
            {
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                {
                    RecommendedAlbums = recommendedAlbums;
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
           
        }

        private async void Albums_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (Locator.MediaLibrary.Albums.Count == 0 || Locator.MediaLibrary.Albums.Count == 1)
                {
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
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
                {
                    await OrderAlbums();
                }
            }
            catch { }
        }

        public async Task OrderAlbums()
        {
            _groupedAlbums = Locator.MediaLibrary.OrderAlbums(Locator.SettingsVM.AlbumsOrderType, Locator.SettingsVM.AlbumsOrderListing);
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(GroupedAlbums));
            });
        }

        private void initializeArtists()
        {
            LoadingStateArtists = LoadingState.Loading;
            GroupedArtists = new ObservableCollection<GroupItemList<ArtistItem>>();

            Locator.MediaLibrary.Artists.CollectionChanged += Artists_CollectionChanged;
            Locator.MediaLibrary.LoadArtistsFromDatabase();
                
            LoadingStateArtists = LoadingState.Loaded;
            OnPropertyChanged(nameof(IsMusicLibraryEmpty));
            OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
        }

        private async void Artists_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Locator.MediaLibrary.Artists.Count == 0 || Locator.MediaLibrary.Artists.Count == 1)
            {
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
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
            _groupedArtists = Locator.MediaLibrary.OrderArtists();
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(GroupedArtists));
            });
        }

        private async Task InitializeTracks()
        {
            LoadingStateTracks = LoadingState.Loading;
            if (Locator.MediaLibrary.Tracks != null)
                Locator.MediaLibrary.Tracks.CollectionChanged += Tracks_CollectionChanged;
            Locator.MediaLibrary.LoadTracksFromDatabase();
            LoadingStateTracks = LoadingState.Loaded;
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
            });
            await OrderTracks();
        }

        private async void Tracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Locator.MediaLibrary.Tracks.Count == 0 || Locator.MediaLibrary.Tracks.Count == 1)
            {
                await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                {
                    OnPropertyChanged(nameof(IsMusicLibraryEmpty));
                    OnPropertyChanged(nameof(MusicLibraryEmptyVisible));
                });
            }

            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems.Count > 0)
            {
                foreach (var newItem in e.NewItems)
                {
                    var track = (TrackItem)newItem;
                    await InsertIntoGroupTrack(track);
                }
            }
        }

        async Task OrderTracks()
        {
            _groupedTracks = Locator.MediaLibrary.OrderTracks();
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(GroupedTracks));
            });
        }

        public void InitializePlaylists()
        {
            LoadingStatePlaylists = LoadingState.Loading;
            Locator.MediaLibrary.LoadPlaylistsFromDatabase();
            OnPropertyChanged(nameof(TrackCollections));
            LoadingStatePlaylists = LoadingState.Loaded;
        }
        #region methods            

        Task InsertIntoGroupAlbum(AlbumItem album)
        {
            try
            {
                return DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
                {
                    var index = -1;
                    if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByArtist)
                    {
                        index = GroupedAlbums.IndexOf(GroupedAlbums.LastOrDefault(x => string.Compare(x.Artist, album.Artist, StringComparison.CurrentCultureIgnoreCase) < 0));
                    }
                    else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByDate)
                    {
                        index = GroupedAlbums.IndexOf(GroupedAlbums.LastOrDefault(x => x.Year < album.Year));
                    }
                    else if (Locator.SettingsVM.AlbumsOrderType == OrderType.ByAlbum)
                    {
                        index = GroupedAlbums.IndexOf(GroupedAlbums.LastOrDefault(x => string.Compare(x.Name, album.Name, StringComparison.CurrentCultureIgnoreCase) < 0));
                    }

                    if (index == -1)
                        index = 0;
                    else
                        index = index + 1;

                    GroupedAlbums.Insert(index, album);
                });
            }
            catch (Exception e)
            {
                LogHelper.Log(e.ToString());
            }
            return Task.FromResult(false);
        }

        async Task InsertIntoGroupArtist(ArtistItem artist)
        {
            try
            {
                var supposedFirstChar = Strings.HumanizedArtistFirstLetter(artist.Name);
                var firstChar = GroupedArtists.FirstOrDefault(x => (string)x.Key == supposedFirstChar);
                if (firstChar == null)
                {
                    var newChar = new GroupItemList<ArtistItem>(artist) { Key = supposedFirstChar };
                    if (GroupedArtists == null)
                        return;
                    int i = GroupedArtists.IndexOf(GroupedArtists.LastOrDefault(x => string.Compare((string)x.Key, (string)newChar.Key) < 0));
                    i++;
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => GroupedArtists.Insert(i, newChar));
                }
                else
                {
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => firstChar.Add(artist));
                }
            }
            catch { }
        }

        async Task InsertIntoGroupTrack(TrackItem track)
        {
            try
            {
                var supposedFirstChar = Strings.HumanizedArtistFirstLetter(track.Name);
                var firstChar = GroupedTracks.FirstOrDefault(x => (string)x.Key == supposedFirstChar);
                if (firstChar == null)
                {
                    var newChar = new GroupItemList<TrackItem>(track) { Key = supposedFirstChar };
                    if (GroupedTracks == null)
                        return;
                    int i = GroupedTracks.IndexOf(GroupedTracks.LastOrDefault(x => string.Compare((string)x.Key, (string)newChar.Key) < 0));
                    i++;
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => GroupedTracks.Insert(i, newChar));
                }
                else
                {
                    await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () => firstChar.Add(track));
                }
            }
            catch { }
        }
        #endregion
        public void GoBack()
        {
            if (_musicView == MusicView.Artists)
                Locator.MusicLibraryVM.ResetCurrentArtistAndAlbumCommand.Execute(null);
        }
    }
}
