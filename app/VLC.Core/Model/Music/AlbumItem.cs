
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Autofac;
using SQLite;
using VLC.Commands.MusicLibrary;
using VLC.Commands.MusicPlayer;
using VLC.Helpers;
using VLC.Helpers.MusicLibrary;
using VLC.Services.RunTime;
using VLC.Utils;
using VLC.ViewModels;
using System.Collections.Generic;

namespace VLC.Model.Music
{

    public class AlbumItem : BindableBase
    {
        private string _name;
        private string _artist;
        private string _albumArtist;
        private string _picture;
        private BitmapImage _albumImage;
        private LoadingState _albumImageLoadingState = LoadingState.NotLoaded;
        private int _year;
        private bool _favorite;
        private bool _isTracksLoaded = false;
        private List<TrackItem> _trackItems;
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

        public string AlbumArtist
        {
            get { return _albumArtist; }
            set { SetProperty(ref _albumArtist, value); }
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
        public AlbumTrackClickedCommand AlbumTrackClickedCommand { get; } = new AlbumTrackClickedCommand();

        [Ignore]
        public PinAlbumCommand PinAlbumCommand { get; } = new PinAlbumCommand();

        [Ignore]
        public SeeArtistShowsCommand SeeArtistShowsCommand { get; } = new SeeArtistShowsCommand();

        [Ignore]
        public List<TrackItem> Tracks
        {
            get
            {
                if (!_isTracksLoaded)
                {
                    _isTracksLoaded = true;
                    Task.Run(() => Locator.MediaLibrary.PopulateTracks(this)).ConfigureAwait(false);
                }
                return _trackItems ?? (_trackItems = new List<TrackItem>());
            }
            set { SetProperty(ref _trackItems, value); }
        }


        public string AlbumCoverUri
        {
            get
            {
                if (!string.IsNullOrEmpty(_picture))
                    return _picture;
                return null;
            }
            set
            {
                SetProperty(ref _picture, value);
            }
        }

        /// <summary>
        /// Returns the full Album cover Uri with the correct ms-appdata prefix.
        /// <returns></returns>
        /// Should be used on runtime only, and not in XAML.
        /// For XAML displaying, consider using AlbumCoverImage
        /// </summary>
        [Ignore]
        public string AlbumCoverFullUri
        {
            get
            {
                if (!string.IsNullOrEmpty(_picture))
                    return "ms-appdata:///local/" + _picture;
                return null;
            }
        }

        /// <summary>
        /// If the CoverImage bitmap is not loaded, it spawns a thread which will wait through a mutex, and try to get the picture from the local folders or the Internet.
        /// <returns></returns>
        /// Then, the property is refreshed and the XAML receives the OnNotifyPropertyChanged event, updating the picture and displaying it
        /// </summary>
        [Ignore] // When we need Cover BitmapImage on runtime
        public BitmapImage AlbumImage
        {
            get
            {
                if (_albumImage == null && _albumImageLoadingState == LoadingState.NotLoaded)
                {
                    _albumImageLoadingState = LoadingState.Loading;
                    ResetAlbumArt().Wait();
                }
                return _albumImage;
            }
            set { SetProperty(ref _albumImage, value); }
        }

        public Task ResetAlbumArt()
        {
            return LoadImageToMemoryHelper.LoadImageToMemory(this);
        }

        [Ignore]
        public bool IsPictureLoaded => (!string.IsNullOrEmpty(_picture));

        public int Year
        {
            get { return _year; }
            set { SetProperty(ref _year, value); }
        }

        public bool IsPinned
        {
            get { return _isPinned; }
            set { SetProperty(ref _isPinned, value); }
        }
    }
}
