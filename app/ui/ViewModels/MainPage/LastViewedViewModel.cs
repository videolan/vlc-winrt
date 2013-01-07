using System.Collections.ObjectModel;
using VLC_WINRT.Common;

namespace VLC_WINRT.ViewModels.MainPage
{
    public class LastViewedViewModel : BindableBase
    {
        private ObservableCollection<MediaViewModel> _lastViewed;

        public ObservableCollection<MediaViewModel> LastViewed
        {
            get { return _lastViewed; }
            set { SetProperty(ref _lastViewed, value); }
        }
    }
}