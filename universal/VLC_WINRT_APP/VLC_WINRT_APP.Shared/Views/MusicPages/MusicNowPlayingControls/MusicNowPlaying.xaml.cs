using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MusicPages.MusicNowPlayingControls
{
    public sealed partial class MusicNowPlaying : UserControl
    {
        private int selectedTrack;
        public MusicNowPlaying()
        {
            this.InitializeComponent();
        }
        private async void PlayPauseHold(object sender, HoldingRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.PlayOrPauseCommand.Execute(null);
            await Locator.MusicPlayerVM.CleanViewModel();
            if (App.ApplicationFrame.CanGoBack)
                App.ApplicationFrame.GoBack();
        }
    }
}
