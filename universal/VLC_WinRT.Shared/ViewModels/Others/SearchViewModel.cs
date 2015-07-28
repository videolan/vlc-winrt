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
                if (string.IsNullOrEmpty(_searchTag) && !string.IsNullOrEmpty(value))
                {
                    Locator.NavigationService.Go(VLCPage.SearchPage);
                }
                if (!string.IsNullOrEmpty(value) && value.Length > 1)
                    SearchHelpers.SearchAlbums(value, SearchResultsAlbums);
                if (!string.IsNullOrEmpty(value) && value.Length > 1)
                    SearchHelpers.SearchVideos(value, SearchResultsVideos);
                SetProperty(ref _searchTag, value);
            }
        }
    }
}
