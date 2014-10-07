/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages.VideoPanes;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageVideos : Page
    {
        public MainPageVideos()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && Locator.VideoLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.VideoLibraryVM.Initialize();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (Window.Current.Bounds.Width < 400)
            {
                RootGrid.Margin = new Thickness(9, 0, 0, 0);
                FirstRowDefinition.Height = new GridLength(0);
            }
            else
            {
                RootGrid.Margin = new Thickness(40, 0, 0, 0);
                FirstRowDefinition.Height = new GridLength(42);
            }
        }

        private void VideoPanesFrame_OnLoadedcPanesFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (VideoPanesFrame.CurrentSourcePageType == null)
                VideoPanesFrame.Navigate(typeof(VideosPage));
        }

        private void Panels_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var panel = e.ClickedItem as Model.Panel;
            foreach (var panel1 in Locator.VideoLibraryVM.Panels)
            {
                panel1.Color = new SolidColorBrush(Colors.DimGray);
            }
            panel.Color = App.Current.Resources["MainColor"] as SolidColorBrush;

            switch (panel.Index)
            {
                case 0:
                    if (VideoPanesFrame.CurrentSourcePageType != typeof(VideosPage))
                        VideoPanesFrame.Navigate(typeof(VideosPage));
                    break;
                case 1:
                    if (VideoPanesFrame.CurrentSourcePageType != typeof(TVShowTwoPanes))
                        VideoPanesFrame.Navigate(typeof(TVShowTwoPanes));
                    break;
            }
        }
    }
}
