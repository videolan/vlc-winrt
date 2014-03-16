/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Model;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Helpers.MusicLibrary;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels.MainPage;
using Album = VLC_WINRT.Utility.Helpers.MusicLibrary.MusicEntities.Album;
using Artist = VLC_WINRT.Utility.Helpers.MusicLibrary.MusicEntities.Artist;

namespace VLC_WINRT.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PlayMusic : Page
    {
        public PlayMusic()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FadeInPage.Begin();
        }

        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            await FadeOutPage.BeginAsync();
            NavigationService.NavigateTo(typeof(MainPage));
        }

        private void PopularItemGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            // TODO: For snap view, send the user to the LastFM page. Otherwise, open a popup with the album info.
            var topAlbum = e.ClickedItem as Album;
            /* If, for whatever reason, the album clicked on is null or it does not have a url attached,
             * return back to the view.*/
            if (topAlbum == null)
            {
                Debug.WriteLine("TopAlbum is null.");
                return;
            }
            if (string.IsNullOrEmpty(topAlbum.Url))
            {
                Debug.WriteLine("Album does not have a URL link out.");
                return;
            }
            // LastFM does not append "http" to its URLs sometimes, which can cause Windows to throw an error.
            // So let's check before if it has it or not, and if not we'll append it.
            string appendHttp = !topAlbum.Url.Contains("http://") ? "http://" + topAlbum.Url : topAlbum.Url;
            var launchUri = new Uri(appendHttp);
            Launcher.LaunchUriAsync(launchUri);
        }

        private async void SimilarArtistsGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            // TODO: For snap view, send the user to the LastFM page. Otherwise, open a popup with the artist info.
            var topArtist = e.ClickedItem as Artist;
            /* If, for whatever reason, the album clicked on is null or it does not have a url attached,
             * return back to the view.*/
            if (topArtist == null)
            {
                Debug.WriteLine("TopArtist is null.");
                return;
            }
            if (string.IsNullOrEmpty(topArtist.Url))
            {
                Debug.WriteLine("Artist does not have a URL link out.");
                return;
            }
            await ArtistInformationsHelper.GetArtistFromXboxMusic(topArtist.Name);
            // LastFM does not append "http" to its URLs sometimes, which can cause Windows to throw an error.
            // So let's check before if it has it or not, and if not we'll append it.
            string appendHttp = !topArtist.Url.Contains("http://") ? "http://" + topArtist.Url : topArtist.Url;
            var launchUri = new Uri(appendHttp);
            Launcher.LaunchUriAsync(launchUri);
        }
    }
}
