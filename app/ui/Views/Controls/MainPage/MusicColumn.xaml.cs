/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using Windows.UI.Popups;
using VLC_WINRT.Utility.Commands;

namespace VLC_WINRT.Views.Controls.MainPage
{
    public sealed partial class MusicColumn : UserControl
    {
        private int _currentSection;
        private bool _isLoaded;
        public MusicColumn()
        {
            this.InitializeComponent();
            this.Loaded += (sender, args) =>
            {
                if (_isLoaded) return;
                UIAnimationHelper.FadeOut(TracksPanel);
                for (int i = 0; i < SectionsGrid.Children.Count; i++)
                {
                    if (i != _currentSection)
                        UIAnimationHelper.FadeOut(SectionsGrid.Children[i]);
                }
                _isLoaded = true;
            };
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
            {
                if (sizeChangedEventArgs.NewSize.Width < 1080)
                {
                    SectionsGrid.Margin = new Thickness(40, 0, 0, 0);
                }
                else
                {
                    SectionsGrid.Margin = new Thickness(50, 0, 0, 0);
                }


                if (sizeChangedEventArgs.NewSize.Width == 320)
                {
                    FullGrid.Visibility = Visibility.Collapsed;
                    SnapGrid.Visibility = Visibility.Visible;
                    SectionsHeaderListView.Visibility = Visibility.Collapsed;
                    SectionsGrid.Margin = new Thickness(0);
                }
                else
                {
                    FullGrid.Visibility = Visibility.Visible;
                    SnapGrid.Visibility = Visibility.Collapsed;
                    SectionsHeaderListView.Visibility = Visibility.Visible;
                    SectionsGrid.Margin = new Thickness(50, 0, 0, 0);
                }
            });
        }

        private void AlbumGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as MusicLibraryViewModel.AlbumItem;

            album.PlayAlbum.Execute(e.ClickedItem);
        }

        private void OnSelectedArtist_Changed(object sender, SelectionChangedEventArgs e)
        {
            var artist = e.AddedItems[0] as MusicLibraryViewModel.ArtistItemViewModel;
            AlbumsByArtistListView.ScrollIntoView(artist);
        }

        private void SectionsHeaderListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            int i = ((Model.Panel)e.ClickedItem).Index;
            ChangedSectionsHeadersState(i);
        }
        public void ChangedSectionsHeadersState(int i)
        {
            AlbumsByArtistSemanticZoom.IsZoomedInViewActive = true;
            AlbumsByArtistSnapSemanticZoom.IsZoomedInViewActive = true;
            if (_currentSection == i) return;
            UIAnimationHelper.FadeOut(SectionsGrid.Children[_currentSection]);
            UIAnimationHelper.FadeIn(SectionsGrid.Children[i]);
            _currentSection = i;
            for (int j = 0; j < SectionsHeaderListView.Items.Count; j++)
                Locator.MainPageVM.MusicLibraryVm.Panels[j].Opacity = 0.4;
            Locator.MainPageVM.MusicLibraryVm.Panels[i].Opacity = 1;
        }

        private void AlbumsByArtistSemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            try
            {
                Locator.MusicLibraryVM.ExecuteSemanticZoom();
            }
            catch { }
        }

        private void FavoriteAlbumItemClick(object sender, ItemClickEventArgs e)
        {
            (e.ClickedItem as MusicLibraryViewModel.AlbumItem).PlayAlbum.Execute(e.ClickedItem);
        }
        private void OnHeaderSemanticZoomClicked(object sender, RoutedEventArgs e)
        {
            AlbumsByArtistSnapSemanticZoom.IsZoomedInViewActive = false;
            AlbumsByArtistSemanticZoom.IsZoomedInViewActive = false;
        }
    }
}
