using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Documents;
using VLC.Helpers;
using VLC.Model;
using VLC.UI.Views.MainPages;
using VLC.UI.Views.MusicPages;
using VLC.UI.Views.MusicPages.TagEditorPages;
using VLC.UI.Views.VariousPages;
using VLC.ViewModels;
using VLC.UI.Views.MainPages;
using VLC.UI.Views.MusicPages;
using VLC.UI.Views.MusicPages.AlbumPageControls;
using VLC.UI.Views.MusicPages.ArtistPages;
using VLC.UI.Views.MusicPages.PlaylistControls;
using VLC.UI.Views.VariousPages;
using VLC.UI.Views.VideoPages;
using VLC.UI.Views.VideoPages.VideoNowPlayingControls;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Controls;
using VLC.UI.Views.MusicPages.ArtistPageControls;
using VLC.Utils;
using Panel = VLC.Model.Panel;
using Windows.UI.Core;
using VLC.UI.Views.VideoPages.TVShowsViews;
using VLC.UI.VariousPages;
using VLC.UI.Views.SettingsPages;
using VLC.UI.UWP.Views.VariousPages;

namespace VLC.Services.RunTime
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
            SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            {
                e.Handled = GoBack_Specific();
            };

            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            }

            App.RootPage.NavigationFrame.Navigated += NavigationFrame_Navigated;
            App.SplitShell.FlyoutNavigated += SplitShell_FlyoutNavigated;
            App.SplitShell.FlyoutClosed += SplitShell_FlyoutClosed;
            HomePageNavigated += NavigationService_HomePageNavigated;
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
            ShowBackButtonIfCanGoBack();
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            e.Handled = GoBack_Specific();
        }

        public void ShowBackButtonIfCanGoBack()
        {
            if (CanGoBack())
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            else
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }


        public bool GoBack_Specific()
        {
            switch (CurrentPage)
            {
                case VLCPage.None:
                    break;
                case VLCPage.MainPageXBOX:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.MainPageVideo:
                case VLCPage.MainPageMusic:
                case VLCPage.MainPageNetwork:
                    if (Locator.SettingsVM.MediaCenterMode)
                    {
                        GoBack_Default();
                    }
                    else
                        return false;
                    break;
                case VLCPage.MainPageFileExplorer:
                    if (Locator.FileExplorerVM.CurrentStorageVM != null || Locator.FileExplorerVM.CanGoBack)
                        Locator.FileExplorerVM.GoBackCommand.Execute(null);
                    else
                    {
                        if (Locator.SettingsVM.MediaCenterMode)
                        {
                            GoBack_Default();
                        }
                        else
                            return false;
                    }
                    break;
                case VLCPage.AlbumPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.ArtistPage:
                case VLCPage.ArtistInfoView:
                    GoBack_Default();
                    break;
                case VLCPage.PlaylistPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.CurrentPlaylistPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.VideoPlayerPage:
                    if (currentFlyout == VLCPage.VideoPlayerOptionsPanel)
                        GoBack_HideFlyout();

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
                    Go(Locator.SettingsVM.HomePage);
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
                case VLCPage.AboutAppView:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.ExternalStorageInclude:
                    GoBack_HideFlyout();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }

        public bool CanGoBack()
        {
            if (IsFlyout(CurrentPage))
                return true;
            if (IsCurrentPageAMainPage() && !Locator.SettingsVM.MediaCenterMode)
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

            if (currentFlyout != null && currentFlyout != VLCPage.None)
            {
                Go(currentFlyout);
            }
        }

        public void Go(VLCPage desiredPage)
        {
            if (!IsFlyout(desiredPage) && desiredPage == CurrentPage) return;

            if (IsFlyout(desiredPage))
                currentFlyout = desiredPage;

            switch (desiredPage)
            {
                case VLCPage.MainPageXBOX:
                    App.SplitShell.FlyoutContent = typeof(MainPageXBOX);
                    break;
                case VLCPage.MainPageVideo:
                case VLCPage.MainPageMusic:
                case VLCPage.MainPageFileExplorer:
                case VLCPage.MainPageNetwork:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(HomePage))
                        App.ApplicationFrame.Navigate(typeof(HomePage));

                    HomePageNavigated?.Invoke(null, desiredPage);
                    currentHomePage = desiredPage;
                    break;
                case VLCPage.AlbumPage:
                    App.SplitShell.FlyoutContent = typeof(AlbumPageBase);
                    break;
                case VLCPage.ArtistPage:
                    if (Locator.SettingsVM.MediaCenterMode || CurrentPage != VLCPage.MainPageMusic || Locator.MusicLibraryVM.MusicView != Model.Music.MusicView.Artists
                        || (CurrentPage == VLCPage.MainPageMusic && Locator.MusicLibraryVM.MusicView == Model.Music.MusicView.Artists && Window.Current.Bounds.Width < 750))
                    {
                        if (CurrentPage == VLCPage.ArtistPage)
                        {
                            (App.ApplicationFrame.Content as ArtistPageBase).SetView(false);
                        }
                        else
                        {
                            App.ApplicationFrame.Navigate(typeof(ArtistPageBase), desiredPage);
                        }
                    }
                    break;
                case VLCPage.SearchPage:
                    App.SplitShell.FlyoutContent = typeof(SearchPage);
                    break;
                case VLCPage.ArtistInfoView:
                    if (CurrentPage == VLCPage.ArtistPage)
                    {
                        (App.ApplicationFrame.Content as ArtistPageBase).SetView(true);
                    }
                    else
                    {
                        App.ApplicationFrame.Navigate(typeof(ArtistPageBase), desiredPage);
                    }
                    CurrentPage = desiredPage;
                    break;
                case VLCPage.SettingsPage:
                case VLCPage.SettingsPageUI:
                case VLCPage.SettingsPageMusic:
                case VLCPage.SettingsPageVideo:
                    App.SplitShell.FlyoutContent = typeof(SettingsPage);
                    break;
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
                case VLCPage.AboutAppView:
                    App.SplitShell.FlyoutContent = typeof(AboutPage);
                    break;
                case VLCPage.ExternalStorageInclude:
                    App.SplitShell.FlyoutContent = typeof(ExternalStorageIncludePage);
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
                   page == VLCPage.TrackEditorPage ||
                   page == VLCPage.AboutAppView ||
                   page == VLCPage.SearchPage ||
                   page == VLCPage.MainPageXBOX ||
                   page == VLCPage.ExternalStorageInclude;
        }

        Type VLCPageToPageType(VLCPage p)
        {
            switch (p)
            {
                case VLCPage.None:
                    break;
                case VLCPage.MainPageVideo:
                    return typeof(MainPageVideos);
                case VLCPage.MainPageMusic:
                    return typeof(MainPageMusic);
                case VLCPage.MainPageFileExplorer:
                    return typeof(MainPageFileExplorer);
                case VLCPage.MainPageNetwork:
                    return typeof(MainPageNetwork);
                case VLCPage.AlbumPage:
                    break;
                case VLCPage.ArtistPage:
                    break;
                case VLCPage.ArtistInfoView:
                    break;
                case VLCPage.PlaylistPage:
                    break;
                case VLCPage.CurrentPlaylistPage:
                    break;
                case VLCPage.VideoPlayerPage:
                    break;
                case VLCPage.MusicPlayerPage:
                    break;
                case VLCPage.SettingsPage:
                    break;
                case VLCPage.SpecialThanksPage:
                    break;
                case VLCPage.ArtistShowsPage:
                    break;
                case VLCPage.AddAlbumToPlaylistDialog:
                    break;
                case VLCPage.CreateNewPlaylistDialog:
                    break;
                case VLCPage.LicensePage:
                    break;
                case VLCPage.SearchPage:
                    break;
                case VLCPage.MiniPlayerView:
                    break;
                case VLCPage.SettingsPageUI:
                    break;
                case VLCPage.SettingsPageMusic:
                    break;
                case VLCPage.SettingsPageVideo:
                    break;
                case VLCPage.VideoPlayerOptionsPanel:
                    break;
                case VLCPage.TrackEditorPage:
                    break;
                case VLCPage.FeedbackPage:
                    break;
                case VLCPage.TvShowView:
                    break;
                case VLCPage.AboutAppView:
                    break;
                case VLCPage.MainPageXBOX:
                    break;
                case VLCPage.ExternalStorageInclude:
                    break;
                default:
                    break;
            }
            return null;
        }

        VLCPage PageTypeToVLCPage(Type page)
        {
            if (page == typeof(HomePage))
            {
                if (Locator.MainVM.CurrentPanel != null)
                    return Locator.MainVM.CurrentPanel.Target;
                else
                    return Locator.SettingsVM.HomePage;
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
