using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using VLC_WinRT.Helpers;
using VLC_WinRT.Model;
using VLC_WinRT.UI.Legacy.Views.MainPages;
using VLC_WinRT.UI.Legacy.Views.MusicPages;
using VLC_WinRT.UI.Legacy.Views.MusicPages.TagEditorPages;
using VLC_WinRT.UI.Legacy.Views.VariousPages;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;
using VLC_WinRT.Views.MusicPages;
using VLC_WinRT.Views.MusicPages.AlbumPageControls;
using VLC_WinRT.Views.MusicPages.ArtistPages;
using VLC_WinRT.Views.MusicPages.PlaylistControls;
using VLC_WinRT.Views.VariousPages;
using VLC_WinRT.Views.VideoPages;
using VLC_WinRT.UI.Legacy.Views.VideoPages.VideoNowPlayingControls;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.UI.Legacy.Views.MusicPages.ArtistPageControls;
using VLC_WinRT.Utils;
using Panel = VLC_WinRT.Model.Panel;
using Windows.UI.Core;
using VLC_WinRT.UI.Legacy.Views.VideoPages.TVShowsViews;
#if WINDOWS_UWP
using VLC_WinRT.UI.UWP.Views.SettingsPages;
#else
using VLC_WinRT.UI.Legacy.Views.SettingsPages;
#endif

namespace VLC_WinRT.Services.RunTime
{
    public delegate void HomePageNavigated(object sender, VLCPage homepage);
    public class NavigationService
    {
        public VLCPage CurrentPage { get; private set; }
        private VLCPage currentFlyout { get; set; }

        private VLCPage currentHomePage { get; set; }

        public bool PreventAppExit { get; set; } = false;

        public delegate void Navigated(object sender, VLCPage newPage);


        public Navigated ViewNavigated = delegate { };
        private event HomePageNavigated HomePageNavigated;
        public NavigationService()
        {
#if WINDOWS_APP
#else
#if WINDOWS_UWP
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                e.Handled = true;
                GoBack_Specific();
            };
            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
#endif
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }
#endif
            App.RootPage.NavigationFrame.Navigated += NavigationFrame_Navigated;
            App.SplitShell.FlyoutNavigated += SplitShell_FlyoutNavigated;
            App.SplitShell.FlyoutClosed += SplitShell_FlyoutClosed;
            HomePageNavigated += NavigationService_HomePageNavigated;
            Locator.MainVM.PropertyChanged += MainVM_PropertyChanged;
        }

        private void MainVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainVM.CurrentPanel))
            {
                Go(Locator.MainVM.CurrentPanel.Target);
            }
        }

        private void NavigationService_HomePageNavigated(object sender, VLCPage homepage)
        {
            VLCPageNavigated(homepage);
        }

        private void SplitShell_FlyoutNavigated(object sender, EventArgs p)
        {
            VLCPageNavigated(currentFlyout);
        }

        private void SplitShell_FlyoutClosed(object sender, EventArgs e)
        {
            VLCPageNavigated(PageTypeToVLCPage(App.ApplicationFrame.CurrentSourcePageType));
        }

        private void NavigationFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            VLCPageNavigated(PageTypeToVLCPage(e.SourcePageType));
        }

        void VLCPageNavigated(VLCPage page)
        {
            if (CurrentPage == page) return;
            CurrentPage = page;
            Debug.WriteLine(CurrentPage);
            ViewNavigated.Invoke(null, CurrentPage);

            if (!App.SplitShell.IsTopBarOpen)
                App.SplitShell.ShowTopBar();
#if WINDOWS_UWP
            ShowBackButtonIfCanGoBack();
#endif
        }

#if WINDOWS_APP
#else
        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = true;
            if (Locator.NavigationService.IsPageAMainPage(CurrentPage))
                e.Handled = false;
            GoBack_Specific();
        }

        public void ShowBackButtonIfCanGoBack()
        {
            if (CanGoBack())
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            else
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
#endif

        public void GoBack_Specific()
        {
            switch (CurrentPage)
            {
                case VLCPage.MainPageVideo:
                case VLCPage.MainPageMusic:
                case VLCPage.MainPageFileExplorer:
                case VLCPage.MainPageNetwork:
                case VLCPage.AlbumPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.ArtistPage:
                    GoBack_Default();
                    break;
                case VLCPage.PlaylistPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.CurrentPlaylistPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.VideoPlayerPage:
                    Locator.MediaPlaybackViewModel.GoBack.Execute(null);
                    break;
                case VLCPage.MusicPlayerPage:
                    GoBack_Default();
                    break;
                case VLCPage.SpecialThanksPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.ArtistShowsPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.AddAlbumToPlaylistDialog:
                    Go(VLCPage.AlbumPage);
                    break;
                case VLCPage.CreateNewPlaylistDialog:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.LicensePage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.MiniPlayerView:
                    AppViewHelper.ResizeWindow(true);
                    GoBack_Default();
                    break;
                // Settings pages
                case VLCPage.SettingsPage:
                case VLCPage.SearchPage:
#if WINDOWS_UWP
                    Go(Locator.SettingsVM.HomePage);
#else
                    GoBack_HideFlyout();
#endif
                    break;
                case VLCPage.SettingsPageUI:
                    Go(VLCPage.SettingsPage);
                    break;
                case VLCPage.SettingsPageMusic:
                    Go(VLCPage.SettingsPage);
                    break;
                case VLCPage.SettingsPageVideo:
                    Go(VLCPage.SettingsPage);
                    break;
                case VLCPage.VideoPlayerOptionsPanel:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.TrackEditorPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.FeedbackPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.TvShowView:
                    GoBack_HideFlyout();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool CanGoBack()
        {
            if (IsFlyout(CurrentPage))
                return true;
            if (IsCurrentPageAMainPage())
                return false;
            return App.ApplicationFrame.CanGoBack;
        }

        // Returns false if it can't go back
        public bool GoBack_Default()
        {
            bool canGoBack = CanGoBack();
            if (canGoBack)
            {
                App.ApplicationFrame.GoBack();
            }
            return canGoBack;
        }

        public void GoBack_HideFlyout()
        {
            App.SplitShell.HideFlyout();
        }

        /// <summary>
        /// Refreshes the current page the user is on.
        /// Useful when switching settings such as language.
        /// </summary>
        public void RefreshCurrentPage()
        {
            var frame = App.ApplicationFrame;
            if (frame?.Content == null) return;
            frame.Navigate(frame.Content.GetType());
            frame.GoBack();
        }

        public void Go(VLCPage desiredPage)
        {
            if (!IsFlyout(desiredPage) && desiredPage == CurrentPage) return;

            if (IsFlyout(desiredPage))
                currentFlyout = desiredPage;

            switch (desiredPage)
            {
                case VLCPage.MainPageVideo:
                case VLCPage.MainPageMusic:
                case VLCPage.MainPageFileExplorer:
                case VLCPage.MainPageNetwork:
                case VLCPage.SearchPage:
                    if (Locator.MainVM.CurrentPanel?.Target != desiredPage)
                    {
                        switch (desiredPage)
                        {
                            case VLCPage.SearchPage:
                                Locator.MainVM.Panels.Add(new Panel(Strings.Search, VLCPage.SearchPage, App.Current.Resources["SearchSymbol"].ToString(), App.Current.Resources["SearchFilledSymbol"].ToString()));
                                break;
                        }
                    }

                    Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == desiredPage);

                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(HomePage))
                        App.ApplicationFrame.Navigate(typeof(HomePage));
                    HomePageNavigated?.Invoke(null, desiredPage);
                    currentHomePage = desiredPage;
                    break;
                case VLCPage.AlbumPage:
                    App.SplitShell.FlyoutContent = typeof(AlbumPageBase);
                    break;
                case VLCPage.ArtistPage:
#if WINDOWS_UWP // On UWP, the ArtistPage is embedded in MainpageMusic and Artists MusicView, it is not the case on Windows 9.1
                    if (CurrentPage != VLCPage.MainPageMusic || Locator.MusicLibraryVM.MusicView != Model.Music.MusicView.Artists
                        || (CurrentPage == VLCPage.MainPageMusic && Locator.MusicLibraryVM.MusicView == Model.Music.MusicView.Artists && Window.Current.Bounds.Width < 750))
#endif
                    {
                        App.ApplicationFrame.Navigate(typeof(ArtistPageBase), desiredPage);
                    }
                    break;
                case VLCPage.ArtistInfoView:
                    App.ApplicationFrame.Navigate(typeof(ArtistPageBase), desiredPage);
                    break;
#if WINDOWS_UWP
                case VLCPage.SettingsPage:
                case VLCPage.SettingsPageUI:
                case VLCPage.SettingsPageMusic:
                case VLCPage.SettingsPageVideo:
                    //App.ApplicationFrame.Navigate(typeof(SettingsPage));
                    App.SplitShell.FlyoutContent = typeof(SettingsPage);
                    break;
#else
                // Settings pages
                case VLCPage.SettingsPage:
                    App.SplitShell.FlyoutContent = typeof(SettingsPage);
                    break;
                case VLCPage.SettingsPageUI:
                    App.SplitShell.FlyoutContent = typeof(SettingsPageUI);
                    break;
                case VLCPage.SettingsPageMusic:
                    App.SplitShell.FlyoutContent = typeof(SettingsPageMusic);
                    break;
                case VLCPage.SettingsPageVideo:
                    App.SplitShell.FlyoutContent = typeof(SettingsPageVideo);
                    break;
#endif
                case VLCPage.PlaylistPage:
                    App.SplitShell.FlyoutContent = typeof(PlaylistPage);
                    break;
                case VLCPage.CurrentPlaylistPage:
                    App.SplitShell.FlyoutContent = typeof(MusicPlaylistPage);
                    break;
                case VLCPage.VideoPlayerPage:
                    App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
                    break;
                case VLCPage.MusicPlayerPage:
                    App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
                    break;
                case VLCPage.SpecialThanksPage:
                    App.SplitShell.FlyoutContent = typeof(SpecialThanks);
                    break;
                case VLCPage.ArtistShowsPage:
                    App.SplitShell.FlyoutContent = typeof(ArtistShowsPage);
                    break;
                case VLCPage.AddAlbumToPlaylistDialog:
                    App.SplitShell.FlyoutContent = typeof(AddAlbumToPlaylistBase);
                    break;
                case VLCPage.CreateNewPlaylistDialog:
                    App.SplitShell.FlyoutContent = typeof(CreateNewPlaylist);
                    break;
                case VLCPage.LicensePage:
                    App.SplitShell.FlyoutContent = typeof(LicensePage);
                    break;
                case VLCPage.MiniPlayerView:
                    App.ApplicationFrame.Navigate(typeof(MiniPlayerWindow));
                    break;
                case VLCPage.VideoPlayerOptionsPanel:
                    App.SplitShell.FlyoutContent = typeof(VideoPlayerOptionsPanel);
                    break;
                case VLCPage.TrackEditorPage:
                    App.SplitShell.FlyoutContent = typeof(TrackEditorPage);
                    break;
                case VLCPage.FeedbackPage:
                    App.SplitShell.FlyoutContent = typeof(FeedbackPage);
                    break;
                case VLCPage.TvShowView:
                    App.SplitShell.FlyoutContent = typeof(ShowEpisodesView);
                    break;
                default:
                    break;
            }
            if (App.SplitShell.IsFlyoutOpen && !IsFlyout(desiredPage))
                App.SplitShell.HideFlyout();
        }

        public bool IsFlyout(VLCPage page)
        {
            return page == VLCPage.AlbumPage ||
                   page == VLCPage.AddAlbumToPlaylistDialog ||
                   page == VLCPage.CreateNewPlaylistDialog ||
                   page == VLCPage.ArtistShowsPage ||
                   page == VLCPage.PlaylistPage ||
                   page == VLCPage.LicensePage ||
                   page == VLCPage.SpecialThanksPage ||
                   page == VLCPage.CurrentPlaylistPage ||
                   page == VLCPage.SettingsPageUI ||
                   page == VLCPage.SettingsPageMusic ||
                   page == VLCPage.SettingsPageVideo ||
                   page == VLCPage.SettingsPage ||
                   page == VLCPage.VideoPlayerOptionsPanel ||
                   page == VLCPage.FeedbackPage ||
                   page == VLCPage.TvShowView ||
                   page == VLCPage.TrackEditorPage;
        }

        VLCPage PageTypeToVLCPage(Type page)
        {
            if (page == typeof(HomePage))
            {
                return Locator.MainVM.CurrentPanel.Target;
            }
            if (page == typeof(MainPageVideos))
                return VLCPage.MainPageVideo;
            if (page == typeof(MainPageMusic))
                return VLCPage.MainPageMusic;
            if (page == typeof(MainPageFileExplorer))
                return VLCPage.MainPageFileExplorer;
            if (page == typeof(MainPageNetwork))
                return VLCPage.MainPageNetwork;
            if (page == typeof(PlaylistPage))
                return VLCPage.PlaylistPage;
            if (page == typeof(MusicPlaylistPage))
                return VLCPage.CurrentPlaylistPage;
            if (page == typeof(VideoPlayerPage))
                return VLCPage.VideoPlayerPage;
            if (page == typeof(MusicPlayerPage))
                return VLCPage.MusicPlayerPage;
            if (page == typeof(SettingsPage))
                return VLCPage.SettingsPage;
            if (page == typeof(SearchPage))
                return VLCPage.SearchPage;
            if (page == typeof(TrackEditorPage))
                return VLCPage.TrackEditorPage;
            if (page == typeof(FeedbackPage))
                return VLCPage.FeedbackPage;
            if (page == typeof(ArtistPageBase))
                return VLCPage.ArtistPage;
            if (page == typeof(MiniPlayerWindow))
                return VLCPage.MiniPlayerView;
            return VLCPage.None;
        }

        public int VLCHomePageToPanelIndex(VLCPage p)
        {
            switch (p)
            {
                case VLCPage.MainPageVideo:
                    return 1;
                case VLCPage.MainPageMusic:
                    return 2;
                case VLCPage.MainPageFileExplorer:
                    return 3;
                case VLCPage.MainPageNetwork:
                    return 4;
                default:
                    return 0;
            }
        }

        public bool IsCurrentPageAMainPage()
        {
            return IsPageAMainPage(CurrentPage);
        }

        public bool IsPageAMainPage(VLCPage p)
        {
            return p == VLCPage.MainPageVideo
                   || p == VLCPage.MainPageMusic
                   || p == VLCPage.MainPageFileExplorer
                   || p == VLCPage.MainPageNetwork;
        }
    }
}
