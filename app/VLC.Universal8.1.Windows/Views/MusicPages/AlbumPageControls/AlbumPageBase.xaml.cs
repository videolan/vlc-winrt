using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using VLC.Helpers;

namespace VLC.UI.Views.MusicPages.AlbumPageControls
{
    public sealed partial class AlbumPageBase
    {
        public AlbumPageBase()
        {
            this.InitializeComponent();
        }

        private void AlbumPageBase_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox && 
                ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Control", "XYFocusDown"))
            {
                TracksListView.XYFocusDown = TracksListView;
                TracksListView.XYFocusLeft = TracksListView;
                TracksListView.XYFocusRight = TracksListView;
            }
        }
    }
}
