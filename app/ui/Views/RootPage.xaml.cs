/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.IoC;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.Views
{
    public sealed partial class RootPage : Page
    {
        public RootPage()
        {
            InitializeComponent();
            Loaded += SwapPanelLoaded;
        }

        private async void SwapPanelLoaded(object sender, RoutedEventArgs e)
        {
            var vlcPlayerService = IoC.GetInstance<MediaPlayerService>();
            await vlcPlayerService.Initialize(SwapChainPanel);
        }

        private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            AnimatedBackground.Visibility = e.SourcePageType == typeof (PlayVideo) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
