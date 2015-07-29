using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;

namespace VLC_WinRT.ViewModels.Others
{
    public class SearchViewModel : BindableBase
    {
        private ObservableCollection<AlbumItem> _searchResultsAlbums = new ObservableCollection<AlbumItem>();
        private ObservableCollection<VideoItem> _searchResultsVideos = new ObservableCollection<VideoItem>();

        private string _searchTag;
        private bool _musicSearchEnabled;
        private bool _videoSearchEnabled = true;

        public ObservableCollection<AlbumItem> SearchResultsAlbums
        {
            get { return _searchResultsAlbums; }
            set { SetProperty(ref _searchResultsAlbums, value); }
        }

        public ObservableCollection<VideoItem> SearchResultsVideos
        {
            get { return _searchResultsVideos; }
            set { SetProperty(ref _searchResultsVideos, value); }
        }

        public string SearchTag
        {
            get { return _searchTag; }
            set
            {
                if (MusicSearchEnabled && !string.IsNullOrEmpty(value) && value.Length > 1)
                    SearchHelpers.SearchAlbums(value, SearchResultsAlbums);
                else if (VideoSearchEnabled && !string.IsNullOrEmpty(value) && value.Length > 1)
                    SearchHelpers.SearchVideos(value, SearchResultsVideos);
                SetProperty(ref _searchTag, value);
            }
        }

        public bool MusicSearchEnabled
        {
            get { return _musicSearchEnabled; }
            set
            {
                SetProperty(ref _musicSearchEnabled, value);
                _videoSearchEnabled = !value;
                OnPropertyChanged("VideoSearchEnabled");
                if (value && !string.IsNullOrEmpty(SearchTag))
                {
                    SearchHelpers.SearchAlbums(SearchTag, SearchResultsAlbums);
                }
            }
        }

        public bool VideoSearchEnabled
        {
            get { return _videoSearchEnabled; }
            set
            {
                SetProperty(ref _videoSearchEnabled, value);
                _musicSearchEnabled = !value;
                OnPropertyChanged("MusicSearchEnabled");
                if (value && !string.IsNullOrEmpty(SearchTag))
                {
                    SearchHelpers.SearchVideos(SearchTag, SearchResultsVideos);
                }
            }
        }
    }
}
