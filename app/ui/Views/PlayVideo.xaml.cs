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
    public sealed partial class PlayVideo : BasePage
    {
        public PlayVideo()
        {
            InitializeComponent();
            this.Loaded += PanelLoaded;
        }

        private void PanelLoaded(object sender, RoutedEventArgs e)
        {
            Locator.PlayVideoVM.RegisterPanel(SwapChainPanel);
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

    }
}