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
    public class SearchViewModel : BindableBase, IDisposable
    {
        private ObservableCollection<AlbumItem> _searchResultsAlbums;
        private ObservableCollection<VideoItem> _searchResultsVideos;

        private string _searchTag;
        private bool _musicSearchEnabled;
        private bool _videoSearchEnabled;

        public ObservableCollection<AlbumItem> SearchResultsAlbums
        {
            get { return _searchResultsAlbums; }
            set
            {
                SetProperty(ref _searchResultsAlbums, value);
            }
        }

        public ObservableCollection<VideoItem> SearchResultsVideos
        {
            get { return _searchResultsVideos; }
            set
            {
                SetProperty(ref _searchResultsVideos, value);
            }
        }

        public int ResultsCount
        {
            get
            {
                if (_musicSearchEnabled)
                {
                    return SearchResultsAlbums.Count;
                }
                else if (_videoSearchEnabled)
                {
                    return SearchResultsVideos.Count;
                }
                return 0;
            }
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
                _searchResultsVideos.Clear();
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
                _searchResultsAlbums.Clear();
                OnPropertyChanged("MusicSearchEnabled");
                if (value && !string.IsNullOrEmpty(SearchTag))
                {
                    SearchHelpers.SearchVideos(SearchTag, SearchResultsVideos);
                }
            }
        }

        public SearchViewModel()
        {
        }

        public void OnNavigatedTo()
        {
            _searchResultsAlbums = new ObservableCollection<AlbumItem>();
            _searchResultsVideos = new ObservableCollection<VideoItem>();
            _videoSearchEnabled = true;
            SearchResultsAlbums.CollectionChanged += SearchResults_CollectionChanged;
            SearchResultsVideos.CollectionChanged += SearchResults_CollectionChanged;
        }

        private void SearchResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ResultsCount));
        }

        public void Dispose()
        {
            SearchResultsAlbums.CollectionChanged -= SearchResults_CollectionChanged;
            SearchResultsVideos.CollectionChanged -= SearchResults_CollectionChanged;
            _searchResultsAlbums.Clear();
            _searchResultsAlbums = null;
            _searchResultsVideos.Clear();
            _searchResultsVideos = null;
        }
    }
}
