using Windows.UI.Xaml.Controls;
using VLC.ViewModels;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class SliderControl : UserControl
    {
        public SliderControl()
        {
            this.InitializeComponent();
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaEndReached += PlaybackService_Playback_MediaEndReached;
        }

        private void PlaybackService_Playback_MediaEndReached()
        {
            Locator.MediaPlaybackViewModel.Position = 0f;
        }
    }
}
