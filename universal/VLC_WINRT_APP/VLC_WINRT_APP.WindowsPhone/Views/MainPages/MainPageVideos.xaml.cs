using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages.MainMusicControls;
using VLC_WINRT_APP.Views.MainPages.MainVideoControls;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageVideos : UserControl
    {
        public MainPageVideos()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Unloaded += OnUnloaded;
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (App.ApplicationFrame.CanGoBack)
            {
                backPressedEventArgs.Handled = true;
                App.ApplicationFrame.GoBack();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void MainPageVideoContentPresenter_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (MainPageVideoContentPresenter.CurrentSourcePageType == null)
            {
                Locator.MainVM.ChangeMainPageVideoViewCommand.Execute((int)Locator.SettingsVM.VideoView);
            }
        }
    }
}
