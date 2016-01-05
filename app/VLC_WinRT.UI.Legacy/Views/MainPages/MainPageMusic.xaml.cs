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
using VLC_WinRT.ViewModels;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Views.MainPages.MusicPanes;
using VLC_WinRT.ViewModels.Settings;
using VLC_WinRT.ViewModels.MusicVM;

namespace VLC_WinRT.Views.MainPages
{
    public sealed partial class MainPageMusic : Page
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
            this.Loaded += MainPageMusic_Loaded;
        }
        
        void MainPageMusic_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(Window.Current.Bounds.Width);
            Locator.MusicLibraryVM.OnNavigatedTo();
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += AlbumsCollectionButtons_Unloaded;
        }
        
        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive(e.Size.Width);
        }

        void AlbumsCollectionButtons_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
            Locator.MusicLibraryVM.OnNavigatedFrom();
        }

        void Responsive(double width)
        {
            if (width <= 600)
                VisualStateUtilities.GoToState(this, "Narrow", false);
            else
                VisualStateUtilities.GoToState(this, "Wide", false);
        }

        private void MusicPanesFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (MainPageMusicContentPresenter.Content == null)
            {
                Switch(Locator.MusicLibraryVM.MusicView);
            }
            Locator.MusicLibraryVM.PropertyChanged += MusicLibraryVM_PropertyChanged;
        }

        private void MusicLibraryVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MusicLibraryVM.MusicView))
            {
                Switch(Locator.MusicLibraryVM.MusicView);
            }
        }

        void Switch(MusicView view)
        {
            switch (view)
            {
                case MusicView.Albums:
                    if (!(MainPageMusicContentPresenter.Content is AlbumCollectionBase))
                        MainPageMusicContentPresenter.Content = new AlbumCollectionBase();
                    break;
                case MusicView.Artists:
                    if (!(MainPageMusicContentPresenter.Content is ArtistCollectionBase))
                        MainPageMusicContentPresenter.Content = new ArtistCollectionBase();
                    break;
                case MusicView.Songs:
                    if (!(MainPageMusicContentPresenter.Content is SongsPivotItem))
                        MainPageMusicContentPresenter.Content = new SongsPivotItem();
                    break;
                case MusicView.Playlists:
                    if (!(MainPageMusicContentPresenter.Content is PlaylistPivotItem))
                        MainPageMusicContentPresenter.Content = new PlaylistPivotItem();
                    break;
            }
        }
    }
}
