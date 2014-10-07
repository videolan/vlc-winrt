using System.Collections.ObjectModel;
using VLC_WINRT_APP.Common;

namespace VLC_WINRT_APP.Model.Video
{
    public class TvShow : BindableBase
    {
        #region private props

        private string _showTitle;
        private ObservableCollection<TVEpisodeItem> _episodes = new ObservableCollection<TVEpisodeItem>();

        #endregion
        #region public props
        public string ShowTitle
        {
            get { return _showTitle; }
            private set { SetProperty(ref _showTitle, value); }
        }

        public ObservableCollection<TVEpisodeItem> Episodes
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
