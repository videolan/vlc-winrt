using Windows.Graphics.Display;
using Windows.Media;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;

namespace VLC_WINRT_APP.Views.MainPages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly VlcService _vlcService;
        public MainPage(VlcService vlcService, IMediaService mediaService)
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            _vlcService = vlcService;
            (mediaService as MediaService).SetMediaElement(MediaElement);
            (mediaService as MediaService).SetMediaTransportControls(SystemMediaTransportControls.GetForCurrentView());
            Loaded += SwapPanelLoaded;
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            DisplayProperties.OrientationChanged += DisplayPropertiesOnOrientationChanged;
        }

        private void DisplayPropertiesOnOrientationChanged(object sender)
        {
            StatusBar sb = StatusBar.GetForCurrentView();
            if (DisplayHelper.IsPortrait())
            {
                sb.ShowAsync();
            }
            else
            {
                sb.HideAsync();
            }
        }

        private async void SwapPanelLoaded(object sender, RoutedEventArgs e)
        {
            await _vlcService.Initialize(SwapChainPanel);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
