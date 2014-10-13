using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageVideos : Page
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
            if (App.RootPage.PanelsView.IsSideBarVisible) return;
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
        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            Locator.VideoLibraryVM.ExecuteSemanticZoom(sender as SemanticZoom, VideoGroupedByAlphaKey);
        }
    }
}
