/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using Windows.Devices.Input;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages.MusicPanes;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageMusic : Page
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            this.SizeChanged += OnSizeChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
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




        private void Panels_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Model.Panel panel = e.ClickedItem as Model.Panel;
            foreach (var panel1 in Locator.MusicLibraryVM.Panels)
            {
                panel1.Color = new SolidColorBrush(Colors.DimGray);
            }
            panel.Color = App.Current.Resources["MainColor"] as SolidColorBrush;

            switch (panel.Index)
            {
                case 0:
                    if(MusicPanesFrame.CurrentSourcePageType != typeof(ArtistsSemanticZoom))
                        MusicPanesFrame.Navigate(typeof (ArtistsSemanticZoom));
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    if (MusicPanesFrame.CurrentSourcePageType != typeof(ArtistAlbumsTwoPanes))
                        MusicPanesFrame.Navigate(typeof(ArtistAlbumsTwoPanes));
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    if (MusicPanesFrame.CurrentSourcePageType != typeof(SongsListView))
                        MusicPanesFrame.Navigate(typeof(SongsListView));
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    FavoriteAlbumsGridView.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void MusicPanesFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            if(MusicPanesFrame.CurrentSourcePageType == null)
                MusicPanesFrame.Navigate(typeof (ArtistsSemanticZoom));
        }
    }
}
