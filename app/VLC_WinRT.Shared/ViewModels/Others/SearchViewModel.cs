using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Utils;

namespace VLC_WinRT.ViewModels.Others
{
    public class SearchViewModel : BindableBase, IDisposable
    {
        private List<AlbumItem> _searchResultsAlbums;
        private List<VideoItem> _searchResultsVideos;

        private string _searchTag;
        private bool _musicSearchEnabled;
        private bool _videoSearchEnabled;

        public ObservableCollection<AlbumItem> SearchResultsAlbums
        {
            get { return _searchResultsAlbums.ToObservable(); }
        }

        public ObservableCollection<VideoItem> SearchResultsVideos
        {
            get { return _searchResultsVideos.ToObservable(); }
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
                    Task.Run(() => SearchAlbums(value));
                else if (VideoSearchEnabled && !string.IsNullOrEmpty(value) && value.Length > 1)
                    Task.Run(() => SearchVideos(value));
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
                    Task.Run(() => SearchAlbums(SearchTag));
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
                    Task.Run(() => SearchVideos(SearchTag));
                }
            }
        }

        public SearchViewModel()
        {
        }

        public void OnNavigatedTo()
        {
            _searchResultsAlbums = new List<AlbumItem>();
            _searchResultsVideos = new List<VideoItem>();
            _videoSearchEnabled = true;
        }

        private void SearchResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        public void Dispose()
        {
            _searchResultsAlbums.Clear();
            _searchResultsAlbums = null;
            _searchResultsVideos.Clear();
            _searchResultsVideos = null;
        }

        async Task SearchVideos(string tag)
        {
            _searchResultsVideos = await SearchHelpers.SearchVideos(tag, _searchResultsVideos);
            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(SearchResultsVideos));
                OnPropertyChanged(nameof(ResultsCount));
            });
        }

        async Task SearchAlbums(string tag)
        {
            _searchResultsAlbums = await SearchHelpers.SearchAlbums(tag, _searchResultsAlbums);
            await DispatchHelper.InvokeAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(SearchResultsAlbums));
                OnPropertyChanged(nameof(ResultsCount));
            });
        }
    }
}
