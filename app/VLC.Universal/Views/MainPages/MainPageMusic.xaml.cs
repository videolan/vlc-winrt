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
using VLC.ViewModels;
using Microsoft.Xaml.Interactivity;
using VLC.Model.Music;
using VLC.Views.MainPages.MusicPanes;
using VLC.ViewModels.Settings;
using VLC.ViewModels.MusicVM;
using System.Threading.Tasks;

namespace VLC.Views.MainPages
{
    public sealed partial class MainPageMusic : Page
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Responsive(Window.Current.Bounds.Width);
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            await Locator.MusicLibraryVM.OnNavigatedFrom();
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive(e.Size.Width);
        }

        void Responsive(double width)
        {
            if (width <= 600)
                VisualStateUtilities.GoToState(this, "Narrow", false);
            else
                VisualStateUtilities.GoToState(this, "Wide", false);
        }

        private async void MusicPanesFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (MainPageMusicContentPresenter.Content == null)
            {
                await Switch(Locator.MusicLibraryVM.MusicView);
            }
            Locator.MusicLibraryVM.PropertyChanged += MusicLibraryVM_PropertyChanged;
        }

        private async void MusicLibraryVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MusicLibraryVM.MusicView))
            {
                await Switch(Locator.MusicLibraryVM.MusicView);
            }
        }

        async Task Switch(MusicView view)
        {
            await Locator.MusicLibraryVM.OnNavigatedFrom();
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
            await Locator.MusicLibraryVM.OnNavigatedTo();
        }
    }
}
