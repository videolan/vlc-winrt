using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using VLC.Helpers;
using VLC.Model;
using VLC.Model.Music;
using VLC.Model.Video;
using VLC.Utils;

namespace VLC.ViewModels.Others
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
                if (!string.IsNullOrEmpty(value) && value.Length > 1)
                {
                    Task.Run(async () =>
                   {
                       await SearchAlbums(value).ConfigureAwait(false);
                       await SearchVideos(value).ConfigureAwait(false);
                   });
                }
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
                _searchResultsVideos?.Clear();
                OnPropertyChanged(nameof(VideoSearchEnabled));
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
                _searchResultsAlbums?.Clear();
                OnPropertyChanged(nameof(MusicSearchEnabled));
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

        public void TextChanged(string text)
        {
            if (string.IsNullOrEmpty(SearchTag) && !string.IsNullOrEmpty(text))
            {
                if (Locator.NavigationService.CurrentPage == VLCPage.MainPageMusic)
                {
                    MusicSearchEnabled = true;
                }
                else if (Locator.NavigationService.CurrentPage == VLCPage.MainPageVideo)
                {
                    VideoSearchEnabled = true;
                }
                Locator.MainVM.GoToSearchPageCommand.Execute(null);
            }
            else if (string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(SearchTag))
            {
                Locator.NavigationService.GoBack_HideFlyout();
            }
            SearchTag = text;
        }

        private void SearchResults_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        public void Dispose()
        {
            var searchPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == VLCPage.SearchPage);
            if (searchPanel != null)
                Locator.MainVM.Panels.Remove(searchPanel);
            _searchResultsAlbums.Clear();
            _searchResultsAlbums = null;
            _searchResultsVideos.Clear();
            _searchResultsVideos = null;
        }

        async Task SearchVideos(string tag)
        {
            _searchResultsVideos = SearchHelpers.SearchVideos(tag, _searchResultsVideos);
            await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(SearchResultsVideos));
                OnPropertyChanged(nameof(ResultsCount));
            });
        }

        async Task SearchAlbums(string tag)
        {
            _searchResultsAlbums = SearchHelpers.SearchAlbums(tag, _searchResultsAlbums);
            await DispatchHelper.InvokeInUIThread(Windows.UI.Core.CoreDispatcherPriority.Low, () =>
            {
                OnPropertyChanged(nameof(SearchResultsAlbums));
                OnPropertyChanged(nameof(ResultsCount));
            });
        }
    }
}
