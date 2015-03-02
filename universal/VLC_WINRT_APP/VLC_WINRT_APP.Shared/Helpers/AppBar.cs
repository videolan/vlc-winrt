using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;
using VLC_WINRT_APP.Views.MusicPages;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using VLC_WINRT_APP.Model;

namespace VLC_WINRT_APP.Helpers
{
    public static class AppBarHelper
    {
        public static void SetHomeButtonVisibleOrNot()
        {
            if (App.ApplicationFrame.CurrentSourcePageType == typeof (MainPageHome))
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
            if (Locator.MainVM.SecondaryAppBarElements == null || Locator.MainVM.SecondaryAppBarElements.Count != 0)
                return;
            var appbarel = new List<AppBarButton>();
            appbarel.Add(new AppBarButton()
            {
                Label = "special thanks",
                Icon = new SymbolIcon(Symbol.Like),
                Command = Locator.MainVM.GoToThanksPageCommand,
            });
            var settingsVisibility = new Binding()
            {
                Source = Locator.MainVM,
                Path = new PropertyPath("CurrentPage"),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = App.Current.Resources["PageToVisibilityConverter"] as IValueConverter,
                ConverterParameter = "SettingsPage"
            };
            var settingsButton = new AppBarButton()
            {
                Label = "settings",
                Icon = PathHelper.Create(App.Current.Resources["SettingsPath"].ToString()),
                Command = Locator.MainVM.GoToSettingsPageCommand,
            };
            settingsButton.SetBinding(AppBarButton.VisibilityProperty, settingsVisibility);
            appbarel.Add(settingsButton);
            Locator.MainVM.SecondaryAppBarElements = appbarel;
        }

        public static async Task UpdateAppBar(Type page, int index = -1)
        {
            var appbarEl = new List<ICommandBarElement>();
            if (Locator.MainVM.AppBarElements == null) return;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
#if WINDOWS_APP
                if (page == typeof (MainPageHome))
                {
                    appbarEl = SetHomePageButtons(appbarEl);
                }
                else if (page == typeof (MainPageVideos))
                {
                    appbarEl = SetVideoCollectionPageButtons(appbarEl);
                }
                else if (page == typeof (MainPageMusic))
                {
                    appbarEl = SetMusicCollectionPageButtons(appbarEl);
                }
#else
                if (page == typeof (MainPageHome) && index > -1)
                {
                    Locator.MainVM.AppBarElements.Clear();

                    switch (index)
                    {
                        case 0:
                            SetHomePageButtons(appbarEl);
                            break;
                        case 1:
                            SetVideoCollectionPageButtons(appbarEl);
                            break;
                        case 2:
                            SetMusicCollectionPageButtons(appbarEl);
                            break;
                    }
                }
#endif
                else if (page == typeof (ArtistPage))
                {
                    SetArtistPageButtons(appbarEl);
                }
                else if (page == typeof (AlbumPage))
                {
                    SetAlbumPageButtons(appbarEl);
                }
                else if (page == typeof (PlaylistPage))
                {
                    //primarycommands
                    var deleteBut = new AppBarButton()
                    {
                        Icon = PathHelper.Create(App.Current.Resources["MinusPath"].ToString()),
                        Label = "remove track",
                        Name = "deletebutton",
                        Command = Locator.MusicLibraryVM.DeleteSelectedTracksInPlaylistCommand
                    };
                    var deleteButBind = new Binding()
                    {
                        Source = Locator.MusicLibraryVM.CurrentTrackCollection,
                        Converter = App.Current.Resources["NegatedCountToVisibilityConverter"] as IValueConverter,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Path = new PropertyPath("SelectedTracks.Count")
                    };
                    deleteBut.SetBinding(AppBarButton.VisibilityProperty, deleteButBind);

                    var playBut = new AppBarButton()
                    {
                        Name = "playbutton",
                        Icon = PathHelper.Create(App.Current.Resources["PlayPath"].ToString()),
                        Label = "play",
                        Command = Locator.MusicLibraryVM.CurrentTrackCollection.PlayTrackCollCommand,
                        CommandParameter = Locator.MusicLibraryVM.CurrentTrackCollection
                    };
                    // secondary commands
                    var deleteplaylistbutton = new AppBarButton()
                    {
                        Icon = PathHelper.Create(App.Current.Resources["RecyclePath"].ToString()),
                        Label = "delete playlist",
                        Command = Locator.MusicLibraryVM.DeletePlaylistCommand
                    };
                    appbarEl.Add(playBut);
                    appbarEl.Add(deleteBut);
                    appbarEl.Add(deleteplaylistbutton);
                }
                else if (page == typeof (MusicPlayerPage))
                {
                    SetMusicPlayerPageButtons(appbarEl);
                }
                Locator.MainVM.AppBarElements = appbarEl;
            });
        }

        private static List<ICommandBarElement> SetHomePageButtons(List<ICommandBarElement> appbarEl)
        {
            appbarEl.Add(new AppBarButton()
            {
                Label = "search",
                Icon = PathHelper.Create(App.Current.Resources["SearchPath"].ToString()),
                Command = Locator.MainVM.GoToSearchPage
            });
            appbarEl.Add(new AppBarButton()
            {
                Label = "open file",
                Icon = PathHelper.Create(App.Current.Resources["OpenFilePath"].ToString()),
                Command = Locator.VideoLibraryVM.PickVideo
            });
            appbarEl.Add(new AppBarButton()
            {
                Label = "open stream",
                Icon = PathHelper.Create(App.Current.Resources["OnlinePath"].ToString()),
                Flyout = App.Current.Resources["PhoneOpenStreamFlyout"] as Flyout,
            });
            return appbarEl;
        }

        private static List<ICommandBarElement> SetVideoCollectionPageButtons(List<ICommandBarElement> appbarEl)
        {
            appbarEl.Add(new AppBarButton()
            {
                Label = "search",
                Icon = PathHelper.Create(App.Current.Resources["SearchPath"].ToString()),
                Command = Locator.MainVM.GoToSearchPage
            });
            return appbarEl;
        }

        private static List<ICommandBarElement> SetMusicCollectionPageButtons(List<ICommandBarElement> appbarEl)
        {
            appbarEl.Add(new AppBarButton()
            {
                Label = "search",
                Icon = PathHelper.Create(App.Current.Resources["SearchPath"].ToString()),
                Command = Locator.MainVM.GoToSearchPage
            });
            appbarEl.Add(new AppBarButton()
            {
                Label = "random",
                Icon = PathHelper.Create(App.Current.Resources["ShufflePath"].ToString()),
                Command = Locator.MusicLibraryVM.PlayAllRandomCommand
            });
            return appbarEl;
        }

        public static List<ICommandBarElement> SetAlbumPageButtons(List<ICommandBarElement> appbarEl)
        {
            Locator.MainVM.AppBarElements.Clear();
            return appbarEl;
        }

        public static List<ICommandBarElement> SetArtistPageButtons(List<ICommandBarElement> appbarEl)
        {
            Locator.MainVM.AppBarElements.Clear();
            appbarEl.Add(new AppBarButton()
            {
                Label = "play all",
                Icon = PathHelper.Create(App.Current.Resources["PlayPath"].ToString()),
                Command = Locator.MusicLibraryVM.PlayArtistAlbumsCommand,
                CommandParameter = Locator.MusicLibraryVM.CurrentArtist
            });

            // pin artist
            var pinLabelBind = new Binding
            {
                Source = Locator.MusicLibraryVM.CurrentArtist,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = App.Current.Resources["PinConverter"] as IValueConverter,
                ConverterParameter = "text",
                Path = new PropertyPath("IsPinned")
            };
            var pinIconBind = new Binding
            {
                Source = Locator.MusicLibraryVM.CurrentArtist,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Converter = App.Current.Resources["PinConverter"] as IValueConverter,
                ConverterParameter = "icon",
                Path = new PropertyPath("IsPinned")
            };
            var pinButton = new AppBarButton()
            {
                Command = Locator.MusicLibraryVM.CurrentArtist.PinArtistCommand,
                CommandParameter = Locator.MusicLibraryVM.CurrentArtist
            };
            pinButton.SetBinding(AppBarButton.LabelProperty, pinLabelBind);
            pinButton.SetBinding(AppBarButton.IconProperty, pinIconBind);
            appbarEl.Add(pinButton);

            appbarEl.Add(new AppBarButton()
            {
                Label = "shows",
                Icon = new SymbolIcon(Symbol.Calendar),
                Command = Locator.MusicLibraryVM.CurrentArtist.SeeArtistShowsCommand,
                CommandParameter = Locator.MusicLibraryVM.CurrentArtist
            });
            return appbarEl;
        }


        public static List<ICommandBarElement> SetMusicPlayerPageButtons(List<ICommandBarElement> appbarEl)
        {
            Locator.MainVM.AppBarElements.Clear();
            var isCheckedBinding = new Binding
            {
                Source = Locator.MusicPlayerVM.TrackCollection,
                Path = new PropertyPath("IsShuffled")
            };
            var random = new AppBarToggleButton()
            {
                Icon = PathHelper.Create(App.Current.Resources["ShufflePath"].ToString()),
                Command = Locator.MusicPlayerVM.Shuffle,
            };
            random.SetBinding(ToggleButton.IsCheckedProperty, isCheckedBinding);
            appbarEl.Add(random);

            var share = new AppBarButton
            {
                Icon = PathHelper.Create(App.Current.Resources["SharePath"].ToString()),
                Command = Locator.MusicPlayerVM.ShareNowPlayingMusicCommand,
            };
            appbarEl.Add(share);

            var playlist = new AppBarButton
            {
                Icon = new SymbolIcon(Symbol.List),
            };
            appbarEl.Add(playlist);
            return appbarEl;
        }
    }
}
