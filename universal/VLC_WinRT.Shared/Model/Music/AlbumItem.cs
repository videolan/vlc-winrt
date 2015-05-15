
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Autofac;
using SQLite;
using VLC_WinRT.Commands.MusicLibrary;
using VLC_WinRT.Commands.MusicPlayer;
using VLC_WinRT.Helpers;
using VLC_WinRT.Helpers.MusicLibrary;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Model.Music
{

    public class AlbumItem : BindableBase
    {
        private string _name;
        private string _artist;
        private string _picture;
        private BitmapImage _albumImage;
        private LoadingState _albumImageLoadingState = LoadingState.NotLoaded;
        private int _year;
        private bool _favorite;
        private bool _isTracksLoaded = false;
        private ObservableCollection<TrackItem> _trackItems;
        private bool _isPinned;

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public int ArtistId { get; set; }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Artist
        {
            get { return _artist; }
            set
            {
                SetProperty(ref _artist, value);
            }
        }

        public bool Favorite
        {
            get { return _favorite; }
            set
            {
                SetProperty(ref _favorite, value);
            }
        }

        [Ignore]
        public ChangeAlbumArtCommand ChangeAlbumArtCommand { get; } = new ChangeAlbumArtCommand();

        [Ignore]
        public ArtistClickedCommand ViewArtist { get; } = new ArtistClickedCommand();

        [Ignore]
        public PlayAlbumCommand PlayAlbum { get; } = new PlayAlbumCommand();

        [Ignore]
        public FavoriteAlbumCommand FavoriteAlbum { get; } = new FavoriteAlbumCommand();

        [Ignore]
        public AlbumTrackClickedCommand TrackClicked { get; } = new AlbumTrackClickedCommand();

        [Ignore]
        public PinAlbumCommand PinAlbumCommand { get; } = new PinAlbumCommand();

        [Ignore]
        public SeeArtistShowsCommand SeeArtistShowsCommand { get; } = new SeeArtistShowsCommand();

        [Ignore]
        public ObservableCollection<TrackItem> Tracks
        {
            get
            {
                if (!_isTracksLoaded)
                {
                    _isTracksLoaded = true;
                    Task.Run(async () => await this.PopulateTracks());
                }
                return _trackItems ?? (_trackItems = new ObservableCollection<TrackItem>());
            }
            set { SetProperty(ref _trackItems, value); }
        }

        public string Picture
        {
            get { return _picture; }
            set { SetProperty(ref _picture, value); }
        }

        [Ignore]
        public string AlbumCoverUri
        {
            get
            {
                if (!string.IsNullOrEmpty(_picture)) // custom uri
                    return _picture;
                else if (IsPictureLoaded) // default album cover uri
                {
                    if (IsVLCCover)
                        return string.Format("ms-appdata:///local/vlc/art/artistalbum/{0}/{1}/art.jpg", Artist, Name);
                    return string.Format("ms-appdata:///local/albumPic/{0}.jpg", Id);
                }
                return null;
            }
        }

        [Ignore]
        public BitmapImage AlbumImage
        {
            get
            {
                if (_albumImage == null && _albumImageLoadingState == LoadingState.NotLoaded)
                {
                    _albumImageLoadingState = LoadingState.Loading;
                    Task.Run(() => ResetAlbumArt());
                }
                return _albumImage;
            }
            set { SetProperty(ref _albumImage, value); }
        }

        public Task ResetAlbumArt()
        {
            return LoadImageToMemoryHelper.LoadImageToMemory(this);
        }

        public bool IsPictureLoaded { get; set; }
        
        public bool IsVLCCover { get; set; }

        public int Year
        {
            get { return _year; }
            set { SetProperty(ref _year, value); }
        }

        public async Task LoadPicture()
        {
            try
            {
                if (IsPictureLoaded)
                    return;
                Debug.WriteLine("Searching online cover for " + Name);
                await App.MusicMetaService.GetAlbumCover(this);
            }
            catch (Exception)
            {
                // TODO: Tell user we could not get their album art.
                LogHelper.Log("Error getting album art...");
            }
        }


        public bool IsPinned
        {
            get { return _isPinned; }
            set { SetProperty(ref _isPinned, value); }
        }
    }
}
