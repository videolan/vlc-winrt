using Windows.Graphics.Display;
using Windows.Media;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Services.Interface;
using VLC_WINRT_APP.Services.RunTime;
using System;

namespace VLC_WINRT_APP.Views.MainPages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IVlcService _vlcService;
        public MainPage(IVlcService vlcService, IMediaService mediaService)
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            _vlcService = vlcService;
            (mediaService as MediaService).SetMediaElement(MediaElement);
            (mediaService as MediaService).SetMediaTransportControls(SystemMediaTransportControls.GetForCurrentView());
            Loaded += SwapPanelLoaded;
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            DisplayInformation.GetForCurrentView().OrientationChanged += DisplayPropertiesOnOrientationChanged;
        }

        private async void DisplayPropertiesOnOrientationChanged(DisplayInformation info, object sender)
        {
            StatusBar sb = StatusBar.GetForCurrentView();

            var appView = ApplicationView.GetForCurrentView();
            
            if (DisplayHelper.IsPortrait())
            {
                await sb.ShowAsync();
                appView.SuppressSystemOverlays = false;
            }
            else
            {
                await sb.HideAsync();
                appView.SuppressSystemOverlays = true;
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
