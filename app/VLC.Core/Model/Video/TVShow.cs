using System.Collections.ObjectModel;
using System.Linq;
using VLC.Commands.VideoLibrary;
using VLC.Utils;
using Windows.UI.Xaml.Media.Imaging;

namespace VLC.Model.Video
{
    public class TvShow : BindableBase
    {
        #region private props
        private string _showTitle;
        private ObservableCollection<VideoItem> _episodes = new ObservableCollection<VideoItem>();
        private BitmapImage _showImage;

        #endregion
        #region public props
        public string ShowTitle
        {
            get { return _showTitle; }
            private set { SetProperty(ref _showTitle, value); }
        }

        public BitmapImage ShowImage => _showImage ?? (_showImage = Episodes.FirstOrDefault(x => x.HasThumbnail || x.HasMoviePicture)?.VideoImage);
            
        public ObservableCollection<VideoItem> Episodes
        {
            get { return _episodes; }
            set { SetProperty(ref _episodes,value); OnPropertyChanged(nameof(ShowImage)); }
        }

        #endregion

        #region commands
        public static TVShowClickedCommand TVShowClickedCommand => new TVShowClickedCommand();
        #endregion
        #region ctors
        public TvShow(string tvShowName)
        {
            ShowTitle = tvShowName;
        }
        #endregion
    }
}
