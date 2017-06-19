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
using VLC.UI.Views.MainPages.MusicPanes;
using VLC.ViewModels.Settings;
using VLC.ViewModels.MusicVM;
using System.Threading.Tasks;
using VLC.Helpers;

namespace VLC.UI.Views.MainPages
{
    public sealed partial class MainPageMusic : Page
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            Locator.MusicLibraryVM.PropertyChanged -= MusicLibraryVM_PropertyChanged;
            await Locator.MusicLibraryVM.OnNavigatedFrom();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Locator.MusicLibraryVM.PropertyChanged += MusicLibraryVM_PropertyChanged;
        }

        private async void MusicPanesFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            await Switch(Locator.MusicLibraryVM.MusicView);
        }
        
        private async void MusicLibraryVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {           
            if (e.PropertyName == nameof(MusicLibraryVM.MusicView))
            {
                await Switch(Locator.MusicLibraryVM.MusicView);
            }
        }

        private async Task Switch(MusicView view)
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
                    if (!(MainPageMusicContentPresenter.Content is SongCollectionBase))
                        MainPageMusicContentPresenter.Content = new SongCollectionBase();
                    break;
                case MusicView.Playlists:
                    if (!(MainPageMusicContentPresenter.Content is PlaylistCollectionBase))
                        MainPageMusicContentPresenter.Content = new PlaylistCollectionBase();
                    break;
            }
            await Locator.MusicLibraryVM.OnNavigatedTo();
        }
    }
}
