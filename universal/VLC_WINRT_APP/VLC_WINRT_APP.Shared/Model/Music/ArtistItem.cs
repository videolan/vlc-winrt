using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using SQLite;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands.Music;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.MusicMetaFetcher;
using VLC_WINRT_APP.ViewModels.MusicVM;
using VLC_WINRT_APP.MusicMetaFetcher.Models.MusicEntities;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Model.Music
{
    public class ArtistItem : BindableBase
    {
        private string _name;
        private bool _isPictureLoaded;
        private ObservableCollection<AlbumItem> _albumItems;
        private bool _isAlbumsLoaded = false;

        // more informations
        private bool _isFavorite;
        private bool _isOnlinePopularAlbumItemsLoaded = false;
        private List<Album> _onlinePopularAlbumItems;
        private bool _isOnlineRelatedArtistsLoaded = false;
        private List<Artist> _onlineRelatedArtists;
        private bool _isOnlineMusicVideosLoaded = false;
        private string _biography;
        private List<Show> _upcomingShowItems;
        private bool _isUpcomingShowsLoading = false;
        private bool _isUpcomingShowsItemsLoaded = false;
        private PinArtistCommand pinArtistCommand;
        private bool _isPinned;
        private SeeArtistShowsCommand seeArtistShowsCommand;
        private string _genre;
        private BitmapImage _artistImage;
        private LoadingState _artistImageLoadingState = LoadingState.NotLoaded;

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        [Ignore]
        public bool IsOnlinePopularAlbumItemsLoaded
        {
            get { return _isOnlinePopularAlbumItemsLoaded; }
            set { SetProperty(ref _isOnlinePopularAlbumItemsLoaded, value); }
        }

        [Ignore]
        public bool IsOnlineRelatedArtistsLoaded
        {
            get { return _isOnlineRelatedArtistsLoaded; }
            set { SetProperty(ref _isOnlineRelatedArtistsLoaded, value); }
        }

        [Ignore]
        public bool IsOnlineMusicVideosLoaded
        {
            get { return _isOnlineMusicVideosLoaded; }
            set { SetProperty(ref _isOnlineMusicVideosLoaded, value); }
        }

        [Indexed]
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        [Ignore]
        public BitmapImage ArtistImage
        {
            get
            {
                if (_artistImage == null && _artistImageLoadingState == LoadingState.NotLoaded)
                {
                    _artistImageLoadingState = LoadingState.Loading;
                    Task.Run(() => ResetArtistHeader());
                }

                return _artistImage;
            }
            set { SetProperty(ref _artistImage, value); }
        }

        public Task ResetArtistHeader()
        {
            return LoadImageToMemoryHelper.LoadImageToMemory(this);
        }

        [Ignore]
        public string Picture => IsPictureLoaded ? string.Format("ms-appdata:///local/artistPic/{0}.jpg", Id) : null;

        public bool IsPictureLoaded
        {
            get { return _isPictureLoaded; }
            set { SetProperty(ref _isPictureLoaded, value); }
        }

        public async Task LoadPicture()
        {
            try
            {
                if (MemoryUsageHelper.PercentMemoryUsed() > MemoryUsageHelper.MaxRamForResourceIntensiveTasks) return;
                await App.MusicMetaService.GetArtistPicture(this);
            }
            catch (Exception)
            {
                LogHelper.Log("Error getting artist picture : " + _name);
            }
        }

        [Ignore]
        public ObservableCollection<AlbumItem> Albums
        {
            get
            {
                if (!_isAlbumsLoaded)
                {
                    _isAlbumsLoaded = true;
                    Task.Run(async () => await this.PopulateAlbums());
                }
                return _albumItems ?? (_albumItems = new ObservableCollection<AlbumItem>());
            }
            set { SetProperty(ref _albumItems, value); }
        }

        [Ignore]
        public string Biography
        {
            get
            {
                if (_biography != null)
                {
                    return _biography;
                }
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    return "Please verify your internet connection";
                Task.Run(async () => await App.MusicMetaService.GetArtistBiography(this));
                return "Loading";
            }
            set { SetProperty(ref _biography, value); }
        }

        [Ignore]
        public List<Album> OnlinePopularAlbumItems
        {
            get
            {
                if (_onlinePopularAlbumItems != null)
                    return _onlinePopularAlbumItems;
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    Task.Run(() => App.MusicMetaService.GetPopularAlbums(this));
                return null;
            }
            set { SetProperty(ref _onlinePopularAlbumItems, value); }
        }

        [Ignore]
        public List<Artist> OnlineRelatedArtists
        {
            get
            {
                if (_onlineRelatedArtists != null)
                    return _onlineRelatedArtists;
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    Task.Run(() => App.MusicMetaService.GetSimilarArtists(this));
                return null;
            }
            set { SetProperty(ref _onlineRelatedArtists, value); }
        }

        [Ignore]
        public PinArtistCommand PinArtistCommand => pinArtistCommand ?? (pinArtistCommand = new PinArtistCommand());

        public bool IsPinned
        {
            get { return _isPinned; }
            set { SetProperty(ref _isPinned, value); }
        }

        [Ignore]
        public SeeArtistShowsCommand SeeArtistShowsCommand
        {
            get
            {
                return seeArtistShowsCommand ?? (seeArtistShowsCommand = new SeeArtistShowsCommand());
            }
        }

        [Ignore]
        public List<Show> UpcomingShows
        {
            get
            {
                if (!_isUpcomingShowsItemsLoaded)
                {
                    IsUpcomingShowsLoading = true;
                    _isUpcomingShowsItemsLoaded = true;
                    Task.Run(() => App.MusicMetaService.GetArtistEvents(this));
                }
                return _upcomingShowItems;
            }
            set { SetProperty(ref _upcomingShowItems, value); }
        }

        public string Genre
        {
            get { return _genre; }
            set { SetProperty(ref _genre, value); }
        }
        [Ignore]
        public bool IsUpcomingShowsLoading
        {
            get
            {
                return _isUpcomingShowsLoading;
            }
            set { SetProperty(ref _isUpcomingShowsLoading, value); }
        }

        public bool IsFavorite
        {
            get { return _isFavorite; }
            set { SetProperty(ref _isFavorite, value); }
        }
    }
}
