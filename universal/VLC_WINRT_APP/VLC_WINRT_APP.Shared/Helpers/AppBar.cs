using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.MusicPages;

#if WINDOWS_PHONE_APP
namespace VLC_WINRT_APP.Helpers
{
    public static class AppBarHelper
    {
        public static void SetHomeButtonVisibleOrNot()
        {
            if (App.ApplicationFrame.CurrentSourcePageType == typeof(MainPageHome))
            {
                App.RootPage.CommandBar.HomeButtonVisible = false;
            }
            else
            {
                App.RootPage.CommandBar.HomeButtonVisible = true;
            }
        }

        public static void UpdateSecondaryAppBarButtons()
        {
            if (Locator.MainVM.SecondaryAppBarElements == null || Locator.MainVM.SecondaryAppBarElements.Count != 0) return;
            var appbarel = new List<AppBarButton>();
            appbarel.Add(new AppBarButton()
            {
                Label = "special thanks",
                Command = Locator.MainVM.GoToThanksPageCommand,
            });
            appbarel.Add(new AppBarButton()
            {
                Label = "settings",
                Command = Locator.MainVM.GoToSettingsPageCommand
            });
            Locator.MainVM.SecondaryAppBarElements = appbarel;
        }

        public static void UpdateAppBar(Type page, int index = -1)
        {
            var appbarEl = new List<ICommandBarElement>();
            if (Locator.MainVM.AppBarElements == null) return;
            if (page == typeof(MainPageHome) && index > -1)
            {
                Locator.MainVM.AppBarElements.Clear();
                switch (index)
                {
                    case 0:
                        appbarEl.Add(new AppBarButton()
                        {
                            Label = "search",
                            Icon = new SymbolIcon(Symbol.Find),
                            Command = Locator.MainVM.GoToSearchPage
                        });
                        appbarEl.Add(new AppBarButton()
                        {
                            Label = "open file",
                            Icon = new SymbolIcon(Symbol.OpenFile),
                            Command = Locator.VideoLibraryVM.PickVideo
                        });
                        appbarEl.Add(new AppBarButton()
                        {
                            Label = "open stream",
                            Icon = new SymbolIcon(Symbol.World),
                            Flyout = App.Current.Resources["PhoneOpenStreamFlyout"] as Flyout,
                        });
                        break;
                    case 1:
                        appbarEl.Add(new AppBarButton()
                        {
                            Label = "view all",
                            Icon = new SymbolIcon(Symbol.ViewAll),
                            Flyout = App.Current.Resources["PhoneChangeVideoViewFlyout"] as MenuFlyout,
                        });
                        break;
                    case 2:
                        appbarEl.Add(new AppBarButton()
                        {
                            Label = "view all",
                            Icon = new SymbolIcon(Symbol.ViewAll),
                            Flyout = App.Current.Resources["PhoneChangeMusicViewFlyout"] as MenuFlyout,
                        });
                        appbarEl.Add(new AppBarButton()
                        {
                            Label = "random",
                            Icon = new SymbolIcon(Symbol.Shuffle),
                            Command = Locator.MusicLibraryVM.PlayAllRandomCommand
                        });
                        break;
                }
            }
            else if (page == typeof(ArtistPage))
            {
                Locator.MainVM.AppBarElements.Clear();
                appbarEl.Add(new AppBarButton()
                {
                    Label = "play all",
                    Icon = new SymbolIcon(Symbol.Play),
                });
                appbarEl.Add(new AppBarButton()
                {
                    Label = "pin",
                    Icon = new SymbolIcon(Symbol.Pin),
                    Command = Locator.MusicLibraryVM.CurrentArtist.PinArtistCommand,
                    CommandParameter = Locator.MusicLibraryVM.CurrentArtist
                });
                appbarEl.Add(new AppBarButton()
                {
                    Label = "shows",
                    Icon = new SymbolIcon(Symbol.Calendar),
                    Command = Locator.MusicLibraryVM.CurrentArtist.SeeArtistShowsCommand,
                    CommandParameter = Locator.MusicLibraryVM.CurrentArtist
                });
            }
            else if (page == typeof(AlbumPage))
            {
                Locator.MainVM.AppBarElements.Clear();
                appbarEl.Add(new AppBarButton()
                {
                    Label = "add to playlist",
                    Icon = new SymbolIcon(Symbol.Add),
                    Command = Locator.MusicLibraryVM.OpenAddAlbumToPlaylistDialogCommand
                });
                appbarEl.Add(new AppBarButton()
                {
                    Label = "pin album",
                    Icon = new SymbolIcon(Symbol.Pin),
                    Command = Locator.MusicLibraryVM.CurrentAlbum.PinAlbumCommand,
                    CommandParameter = Locator.MusicLibraryVM.CurrentAlbum
                });
                appbarEl.Add(new AppBarButton()
                {
                    Label = "shows",
                    Icon = new SymbolIcon(Symbol.Calendar),
                    Command = Locator.MusicLibraryVM.CurrentArtist.SeeArtistShowsCommand,
                    CommandParameter = Locator.MusicLibraryVM.CurrentArtist
                });
            }
            Locator.MainVM.AppBarElements = appbarEl;
        }
    }
}

#endif