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
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.Utility.Services.Interface;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPage : Page
    {
        private readonly VlcService _vlcService;
        public MainPage(VlcService vlcService, IMediaService mediaService)
        {
            InitializeComponent();
            _vlcService = vlcService;
            (mediaService as MediaService).SetMediaElement(FoudationMediaElement);
            Loaded += SwapPanelLoaded;
        }

        private async void SwapPanelLoaded(object sender, RoutedEventArgs e)
        {
            await _vlcService.Initialize(SwapChainPanel);
        }

        private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            //AnimatedBackground.Visibility = e.SourcePageType == typeof (PlayVideo) ? Visibility.Collapsed : Visibility.Visible;
        }
        //public async void CreateVLCMenu()
        //{
        //    var resourceLoader = new ResourceLoader();
        //    var popupMenu = new PopupMenu();
        //    popupMenu.Commands.Add(new UICommand(resourceLoader.GetString("ExternalStorage"), async h => await ExternalStorage()));

        //    popupMenu.Commands.Add(new UICommand("Media servers", async h => await MediaServers()));


        //    var transform = RootGrid.TransformToVisual(this);
        //    var point = transform.TransformPoint(new Point(270, 110));
        //    await popupMenu.ShowAsync(point);
        //}

        //private void OpenSearchPane(object sender, RoutedEventArgs e)
        //{
        //    App.RootPage.SearchPane.Show();
        //}

        //private async void OpenFile(object sender, RoutedEventArgs e)
        //{
        //    var resourceLoader = new ResourceLoader();
        //    var popupMenu = new PopupMenu();
        //    popupMenu.Commands.Add(new UICommand(resourceLoader.GetString("OpenVideo"), h => OpenVideo()));



        //    var transform = RootGrid.TransformToVisual(this);
        //    var point = transform.TransformPoint(new Point(Window.Current.Bounds.Width - 110, 200));
        //    await popupMenu.ShowAsync(point);
        //}
    }
}
