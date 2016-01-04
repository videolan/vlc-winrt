using System.Collections.ObjectModel;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Model.Video
{
    public class TvShow : BindableBase
    {
        #region private props

        private string _showTitle;
        private ObservableCollection<VideoItem> _episodes = new ObservableCollection<VideoItem>();

        #endregion
        #region public props
        public string ShowTitle
        {
            get { return _showTitle; }
            private set { SetProperty(ref _showTitle, value); }
        }

        public ObservableCollection<VideoItem> Episodes
        {
            get { return _episodes; }
            set { SetProperty(ref _episodes,value); }
        }

        #endregion

        #region ctors
        public TvShow(string tvShowName)
        {
            ShowTitle = tvShowName;
        }
        #endregion
    }
}
