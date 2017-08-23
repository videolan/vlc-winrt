using VLC.ViewModels;
using Windows.UI.Xaml.Controls;
namespace VLC.Universal8._1.Views.UserControls
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
