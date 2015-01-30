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
using Windows.UI.Xaml.Media;
using VLC_WINRT_APP.Model;

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
                Command = Locator.MainVM.GoToSettingsPageCommand,
            };
            settingsButton.SetBinding(AppBarButton.VisibilityProperty, settingsVisibility);
            Locator.MainVM.SecondaryAppBarElements = appbarel;
        }

        public static async Task UpdateAppBar(Type page, int index = -1)
        {
            var appbarEl = new List<ICommandBarElement>();
            if (Locator.MainVM.AppBarElements == null) return;
            await App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (page == typeof(MainPageHome) && index > -1)
                {
                    Locator.MainVM.AppBarElements.Clear();
                    switch (index)
                    {
                        case 0:
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
                            break;
                        case 1:
                            appbarEl.Add(new AppBarButton()
                            {
                                Label = "search",
                                Icon = PathHelper.Create(App.Current.Resources["SearchPath"].ToString()),
                                Command = Locator.MainVM.GoToSearchPage
                            });
                            break;
                        case 2:
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
                            break;
                    }
                }
                else if (page == typeof(ArtistPage))
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
                }
                else if (page == typeof(AlbumPage))
                {
                    Locator.MainVM.AppBarElements.Clear();
                    appbarEl.Add(new AppBarButton()
                    {
                        Label = "add to playlist",
                        Icon = PathHelper.Create(App.Current.Resources["AddPath"].ToString()),
                        Command = Locator.MusicLibraryVM.OpenAddAlbumToPlaylistDialogCommand
                    });


                    // pin artist
                    var pinLabelBind = new Binding
                    {
                        Source = Locator.MusicLibraryVM.CurrentAlbum,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Converter = App.Current.Resources["PinConverter"] as IValueConverter,
                        ConverterParameter = "text",
                        Path = new PropertyPath("IsPinned")
                    };
                    var pinIconBind = new Binding
                    {
                        Source = Locator.MusicLibraryVM.CurrentAlbum,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Converter = App.Current.Resources["PinConverter"] as IValueConverter,
                        ConverterParameter = "icon",
                        Path = new PropertyPath("IsPinned")
                    };
                    var pinButton = new AppBarButton()
                    {
                        Command = Locator.MusicLibraryVM.CurrentAlbum.PinAlbumCommand,
                        CommandParameter = Locator.MusicLibraryVM.CurrentAlbum
                    };
                    pinButton.SetBinding(AppBarButton.LabelProperty, pinLabelBind);
                    pinButton.SetBinding(AppBarButton.IconProperty, pinIconBind);
                    appbarEl.Add(pinButton);

                    var favBut = new AppBarButton()
                    {
                        Command = Locator.MusicLibraryVM.CurrentAlbum.FavoriteAlbum,
                        CommandParameter = Locator.MusicLibraryVM.CurrentAlbum
                    };
                    var favLabelBind = new Binding
                    {
                        Source = Locator.MusicLibraryVM.CurrentAlbum,
                        Converter = App.Current.Resources["FavoriteLabelConverter"] as IValueConverter,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Path = new PropertyPath("Favorite")
                    };
                    var favSymbolBind = new Binding()
                    {
                        Source = Locator.MusicLibraryVM.CurrentAlbum,
                        Converter = App.Current.Resources["FavoriteSymbolConverter"] as IValueConverter,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Path = new PropertyPath("Favorite")
                    };
                    favBut.SetBinding(AppBarButton.LabelProperty, favLabelBind);
                    favBut.SetBinding(AppBarButton.IconProperty, favSymbolBind);
                    appbarEl.Add(favBut);
                    appbarEl.Add(new AppBarButton()
                    {
                        Label = "shows",
                        Icon = new SymbolIcon(Symbol.Calendar),
                        Command = Locator.MusicLibraryVM.CurrentArtist.SeeArtistShowsCommand,
                        CommandParameter = Locator.MusicLibraryVM.CurrentArtist
                    });
                }
                else if (page == typeof(PlaylistPage))
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
                        Icon = new SymbolIcon(Symbol.Remove),
                        Label = "delete playlist",
                        Command = Locator.MusicLibraryVM.DeletePlaylistCommand
                    };
                    appbarEl.Add(playBut);
                    appbarEl.Add(deleteBut);
                    appbarEl.Add(deleteplaylistbutton);
                }
                Locator.MainVM.AppBarElements = appbarEl;
            });
        }
    }
}

#endif