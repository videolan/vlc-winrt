using Windows.UI.Xaml;
using VLC.ViewModels;
using Windows.UI.Xaml.Controls;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class SliderControl : UserControl
    {
        public SliderControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaEndReached -= PlaybackService_Playback_MediaEndReached;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Locator.MediaPlaybackViewModel.PlaybackService.Playback_MediaEndReached += PlaybackService_Playback_MediaEndReached;
        }

        private void PlaybackService_Playback_MediaEndReached()
        {
            Locator.MediaPlaybackViewModel.Position = 0f;
        }
    }
}
