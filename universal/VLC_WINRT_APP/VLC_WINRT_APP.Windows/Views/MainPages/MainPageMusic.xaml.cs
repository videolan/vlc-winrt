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
using Windows.UI.Xaml.Input;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageMusic : Page
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
            this.SizeChanged += OnSizeChanged;
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
                RootGrid.Margin = new Thickness(24, 0, 0, 0);
                FirstRowDefinition.Height = new GridLength(42);
            }
        }

        private void SemanticZoom_OnViewChangeCompletedArtistByName(object sender, SemanticZoomViewChangedEventArgs e)
        {
            Locator.MusicLibraryVM.ExecuteSemanticZoom(SemanticZoomAlbumsByArtist, ArtistsGroupedByName);
        }

        private void SemanticZoomArtistByAlphaKey_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            Locator.MusicLibraryVM.ExecuteSemanticZoom(SemanticZoomArtistByAlphaKey, ArtistByAlphaKey);
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
                panel1.Opacity = 0.4;
            }
            panel.Opacity = 1;
            switch (panel.Index)
            {
                case 0:
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Visible;
                    SemanticZoomArtistByAlphaKey.Visibility = Visibility.Collapsed;
                    RadDataGrid.Visibility = Visibility.Collapsed;
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Collapsed;
                    SemanticZoomArtistByAlphaKey.Visibility = Visibility.Visible;
                    RadDataGrid.Visibility = Visibility.Collapsed;
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case 2:
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Collapsed;
                    SemanticZoomArtistByAlphaKey.Visibility = Visibility.Collapsed;
                    RadDataGrid.Visibility = Visibility.Visible;
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case 3:
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Collapsed;
                    SemanticZoomArtistByAlphaKey.Visibility = Visibility.Collapsed;
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
