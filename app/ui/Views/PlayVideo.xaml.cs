using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

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