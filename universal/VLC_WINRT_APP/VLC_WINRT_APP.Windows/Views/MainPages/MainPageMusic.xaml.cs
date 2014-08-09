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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

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
            if (e.NavigationMode == NavigationMode.New && Locator.MusicLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.MusicLibraryVM.Initialize();
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

        private void SemanticZoom_OnViewChangeCompletedArtistByName(object sender, SemanticZoomViewChangedEventArgs e)
        {
            Locator.MusicLibraryVM.ExecuteSemanticZoom(SemanticZoomAlbumsByArtist, AlbumsGroupedByName);
        }


        private void Header_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SemanticZoomAlbumsByArtist.IsZoomedInViewActive = false;
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
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Visible;
                    ZoomedInListViewArtist.Visibility = Visibility.Collapsed;
                    RadDataGrid.Visibility = Visibility.Collapsed;
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Collapsed;
                    ZoomedInListViewArtist.Visibility = Visibility.Visible;
                    RadDataGrid.Visibility = Visibility.Collapsed;
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Collapsed;
                    ZoomedInListViewArtist.Visibility = Visibility.Collapsed;
                    RadDataGrid.Visibility = Visibility.Visible;
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Collapsed;
                    ZoomedInListViewArtist.Visibility = Visibility.Collapsed;
                    RadDataGrid.Visibility = Visibility.Collapsed;
                    FavoriteAlbumsGridView.Visibility = Visibility.Visible;
                    break;
            }
        }

        //private void RadDataGrid_OnSelectionChanged(object sender, DataGridSelectionChangedEventArgs e)
        //{
        //    PlayTrackCommand playTrackCommand = new PlayTrackCommand();
        //    playTrackCommand.Execute(e);
        //}
    }
}
