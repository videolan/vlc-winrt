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
    public sealed partial class PlayVideo : BasePage, IVideoPage
    {
        public PlayVideo()
        {
            InitializeComponent();
            this.Loaded += ImLoaded;
        }

        private void ImLoaded(object sender, RoutedEventArgs e)
        {
            if (PageLoaded != null)
            {
                PageLoaded(this, e);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModelLocator.PlayVideoVM.SetVideoPage(this);
            base.OnNavigatedTo(e);
        }

        public override void SetDataContext()
        {
            _vm = (NavigateableViewModel) DataContext;
            base.SetDataContext();
        }

        private void ScreenTapped(object sender, TappedRoutedEventArgs e)
        {
            if (BottomAppBar != null && !BottomAppBar.IsOpen)
                BottomAppBar.IsOpen = true;
            if (TopAppBar != null && !TopAppBar.IsOpen)
                TopAppBar.IsOpen = true;
        }

        public SwapChainBackgroundPanel Panel
        {
            get
            {
                return SwapChainPanel;
            }
        }

        public event RoutedEventHandler PageLoaded;
    }

    public interface IVideoPage
    {
        SwapChainBackgroundPanel Panel { get; }
        event RoutedEventHandler PageLoaded;
    }
}