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
using VLC_WINRT.Common;
using VLC_WINRT_APP.Commands;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT.ViewModels;
using VLC_WINRT_APP.ViewModels;
using Panel = VLC_WINRT_APP.Model.Panel;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageMusic : Page
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
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
            switch (panel.Title)
            {
                case "albums":
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Visible;
                    SemanticZoomArtistByAlphaKey.Visibility = Visibility.Collapsed;
                    RadDataGrid.Visibility = Visibility.Collapsed;
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case "artists":
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Collapsed;
                    SemanticZoomArtistByAlphaKey.Visibility = Visibility.Visible;
                    RadDataGrid.Visibility = Visibility.Collapsed;
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case "songs":
                    SemanticZoomAlbumsByArtist.Visibility = Visibility.Collapsed;
                    SemanticZoomArtistByAlphaKey.Visibility = Visibility.Collapsed;
                    RadDataGrid.Visibility = Visibility.Visible;
                    FavoriteAlbumsGridView.Visibility = Visibility.Collapsed;
                    break;
                case "pinned":
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
