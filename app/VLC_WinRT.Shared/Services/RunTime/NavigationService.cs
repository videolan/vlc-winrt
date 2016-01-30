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
using VLC_WinRT.UI.Legacy.Views.SettingsPages;
using VLC_WinRT.UI.Legacy.Views.VariousPages;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;
using VLC_WinRT.Views.MusicPages;
using VLC_WinRT.Views.MusicPages.AlbumPageControls;
using VLC_WinRT.Views.MusicPages.ArtistPageControls;
using VLC_WinRT.Views.MusicPages.ArtistPages;
using VLC_WinRT.Views.MusicPages.PlaylistControls;
using VLC_WinRT.Views.VariousPages;
using VLC_WinRT.Views.VideoPages;
using VLC_WinRT.UI.Legacy.Views.VideoPages.VideoNowPlayingControls;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
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
#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
            App.RootPage.NavigationFrame.Navigated += NavigationFrame_Navigated;
            App.SplitShell.LeftSidebarClosed += SplitShell_LeftSidebarClosed;
            App.SplitShell.RightSidebarNavigated += SplitShell_RightSidebarNavigated;
            App.SplitShell.RightSidebarClosed += SplitShell_RightSidebarClosed;
            HomePageNavigated += NavigationService_HomePageNavigated;
        }

        private void NavigationService_HomePageNavigated(object sender, VLCPage homepage)
        {
            VLCPageNavigated(homepage);
        }
        
        private void SplitShell_RightSidebarNavigated(object sender, EventArgs p)
        {
            VLCPageNavigated(currentFlyout);
        }

        private void SplitShell_RightSidebarClosed(object sender, EventArgs e)
        {
            VLCPageNavigated(PageTypeToVLCPage(App.ApplicationFrame.CurrentSourcePageType));
        }

        private void SplitShell_LeftSidebarClosed(object sender, EventArgs e)
        {
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

            if (Locator.SettingsVM.ApplicationTheme == ApplicationTheme.Light && Locator.Slideshow.IsDarkTheme)
            {
                if (page != VLCPage.MusicPlayerPage && page != VLCPage.CurrentPlaylistPage)
                {
                    Locator.Slideshow.SetTheme(false);
                }
            }
        }

#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            if (App.SplitShell.IsLeftPaneOpen)
            {
                App.SplitShell.CloseLeftPane();
                return;
            }
            if (Locator.NavigationService.IsPageAMainPage(CurrentPage))
                e.Handled = false;
            GoBack_Specific();
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
                case VLCPage.SearchPage:
                    break;
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
                    GoBack_HideFlyout();
                    break;
                case VLCPage.CreateNewPlaylistDialog:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.LicensePage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.MiniPlayerView:
                    AppViewHelper.ResizeWindow(true);
                    App.SplitShell.FooterVisibility = Visibility.Visible;
                    GoBack_Default();
                    Locator.Slideshow.IsPaused = false;
                    break;
                // Settings pages
                case VLCPage.SettingsPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.SettingsPageUI:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.SettingsPageMusic:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.SettingsPageVideo:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.VideoPlayerOptionsPanel:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.TrackEditorPage:
                    GoBack_Default();
                    break;
                case VLCPage.FeedbackPage:
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

        public void Go(VLCPage desiredPage)
        {
            if (!IsFlyout(desiredPage) && desiredPage == CurrentPage) return;
            if (App.SplitShell.IsLeftPaneOpen)
                App.SplitShell.CloseLeftPane();

            if (IsFlyout(desiredPage))
                currentFlyout = desiredPage;

            switch (desiredPage)
            {
                case VLCPage.LeftSidebar:
                    App.SplitShell.OpenLeftPane();
                    CurrentPage = desiredPage;
                    break;
                case VLCPage.MainPageVideo:
                case VLCPage.MainPageMusic:
                case VLCPage.MainPageFileExplorer:
                case VLCPage.MainPageNetwork:
                case VLCPage.SearchPage:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(HomePage))
                        App.ApplicationFrame.Navigate(typeof(HomePage));
                    HomePageNavigated?.Invoke(null, desiredPage);
                    currentHomePage = desiredPage;
                    break;
                case VLCPage.AlbumPage:
                    App.SplitShell.RightFlyoutContent = new AlbumPageBase();
                    break;
                case VLCPage.ArtistPage:
                    App.ApplicationFrame.Navigate(typeof(ArtistPageBase));
                    break;
                case VLCPage.PlaylistPage:
                    App.SplitShell.RightFlyoutContent = new PlaylistPage();
                    break;
                case VLCPage.CurrentPlaylistPage:
                    App.SplitShell.RightFlyoutContent = new MusicPlaylistPage();
                    break;
                case VLCPage.VideoPlayerPage:
                    App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
                    break;
                case VLCPage.MusicPlayerPage:
                    App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
                    break;
                case VLCPage.SpecialThanksPage:
                    App.SplitShell.RightFlyoutContent = new SpecialThanks();
                    break;
                case VLCPage.ArtistShowsPage:
                    App.SplitShell.RightFlyoutContent = new ArtistShowsPage();
                    break;
                case VLCPage.AddAlbumToPlaylistDialog:
                    var addToPlaylist = new AddAlbumToPlaylistBase();
                    App.SplitShell.RightFlyoutContent = addToPlaylist;
                    break;
                case VLCPage.CreateNewPlaylistDialog:
                    var createPlaylist = new CreateNewPlaylist();
                    App.SplitShell.RightFlyoutContent = createPlaylist;
                    break;
                case VLCPage.LicensePage:
                    App.SplitShell.RightFlyoutContent = new LicensePage();
                    break;
                case VLCPage.MiniPlayerView:
                    Locator.Slideshow.IsPaused = true;
                    AppViewHelper.ResizeWindow(false, 400, 80 + AppViewHelper.TitleBarHeight);
                    App.SplitShell.FooterVisibility = Visibility.Collapsed;
                    App.ApplicationFrame.Navigate(typeof(MiniPlayerWindow));
                    break;
                // Settings pages
                case VLCPage.SettingsPage:
                    App.SplitShell.RightFlyoutContent = new SettingsPage();
                    break;
                case VLCPage.SettingsPageUI:
                    App.SplitShell.RightFlyoutContent = new SettingsPageUI();
                    break;
                case VLCPage.SettingsPageMusic:
                    App.SplitShell.RightFlyoutContent = new SettingsPageMusic();
                    break;
                case VLCPage.SettingsPageVideo:
                    App.SplitShell.RightFlyoutContent = new SettingsPageVideo();
                    break;
                case VLCPage.VideoPlayerOptionsPanel:
                    App.SplitShell.RightFlyoutContent = new VideoPlayerOptionsPanel();
                    break;
                case VLCPage.TrackEditorPage:
                    App.ApplicationFrame.Navigate(typeof(TrackEditorPage));
                    break;
                case VLCPage.FeedbackPage:
                    App.SplitShell.RightFlyoutContent = new FeedbackPage();
                    break;
                default:
                    break;
            }
            if (App.SplitShell.IsRightFlyoutOpen && !IsFlyout(desiredPage))
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
                   page == VLCPage.FeedbackPage;
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
