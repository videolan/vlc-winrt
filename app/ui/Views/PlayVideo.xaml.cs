using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PlayVideo : Page
    {
        public PlayVideo()
        {
            InitializeComponent();
            this.Loaded += IntializeVLC;
        }

        private void IntializeVLC(object sender, RoutedEventArgs routedEventArgs)
        {
            ViewModelLocator.PlayVideoVM.InitializeVLC(VLCSwapChainPanel);
            ViewModelLocator.PlayVideoVM.Play();
        }

        /// <summary>
        ///     Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">
        ///     Event data that describes how this page was reached.  The Parameter
        ///     property is typically used to configure the page.
        /// </param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void ScreenTapped(object sender, TappedRoutedEventArgs e)
        {
            if (BottomAppBar != null && !BottomAppBar.IsOpen)
                BottomAppBar.IsOpen = true;
            if (TopAppBar != null && !TopAppBar.IsOpen)
                TopAppBar.IsOpen = true;
        }
    }
}