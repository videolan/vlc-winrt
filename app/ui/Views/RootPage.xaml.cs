/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Autofac;
using System;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Views
{
    public sealed partial class RootPage : Page
    {
        public RootPage()
        {
            InitializeComponent();
            CoreWindow.GetForCurrentThread().Activated += OnActivated;
            Loaded += SwapPanelLoaded;
        }


        private async void OnActivated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                if (!Locator.MusicPlayerVM.TrackCollection.IsRunning)
                    return;
                StorageFile file =
                    await StorageFile.GetFileFromPathAsync(
                        Locator.MusicPlayerVM.TrackCollection.TrackCollection[
                            Locator.MusicPlayerVM.TrackCollection.CurrentTrack].Path);
                IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
                App.RootPage.FoudationMediaElement.SetSource(stream, file.ContentType);
                App.RootPage.FoudationMediaElement.Play();
                App.RootPage.FoudationMediaElement.Volume = 0;
            }
            else
            {
                if (!Locator.MusicPlayerVM.TrackCollection.IsRunning)
                    return;
                App.RootPage.FoudationMediaElement.Stop();
            }
        }

        private async void SwapPanelLoaded(object sender, RoutedEventArgs e)
        {
            var vlcPlayerService = App.Container.Resolve<MediaPlayerService>();
            await vlcPlayerService.Initialize(SwapChainPanel);
        }

        private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
            AnimatedBackground.Visibility = e.SourcePageType == typeof (PlayVideo) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
