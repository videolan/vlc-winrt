using System;
using System.Diagnostics;
using VLC.Helpers;
using VLC.Model;
using VLC.ViewModels;
using VLC.Utils;
using Windows.UI.Core;
using VLC_WinRT.UI.Legacy.Views.MainPages;
using VLC_WinRT.UI.Legacy.Views.MusicPages;
using VLC_WinRT.UI.Legacy.Views.MusicPages.ArtistPageControls;
using VLC_WinRT.UI.Legacy.Views.MusicPages.TagEditorPages;
using VLC_WinRT.UI.Legacy.Views.SettingsPages;
using VLC_WinRT.UI.Legacy.Views.VariousPages;
using VLC_WinRT.UI.Legacy.Views.VideoPages.TVShowsViews;
using VLC_WinRT.UI.Legacy.Views.VideoPages.VideoNowPlayingControls;
using VLC_WinRT.Views.MainPages;
using VLC_WinRT.Views.MusicPages;
using VLC_WinRT.Views.MusicPages.AlbumPageControls;
using VLC_WinRT.Views.MusicPages.ArtistPages;
using VLC_WinRT.Views.MusicPages.PlaylistControls;
using VLC_WinRT.Views.VariousPages;
using VLC_WinRT.Views.VideoPages;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace VLC.Services.RunTime
{
    public delegate void HomePageNavigated(object sender, VLCPage homepage);
    public class NavigationService
    {
        public VLCPage CurrentPage { get; private set; }
        private VLCPage currentFlyout { get; set; }

        public bool PreventAppExit { get; set; } = false;

        public delegate void Navigated(object sender, VLCPage newPage);

        private VLCPage? _playbackStartedTarget = null;
        private object _playbackStartedParam = null;

        public Navigated ViewNavigated = delegate { };
        private event HomePageNavigated HomePageNavigated;
        public NavigationService()
        {
            //SystemNavigationManager.GetForCurrentView().BackRequested += (s, e) =>
            //{
            //    e.Handled = GoBack_Specific();
            //};

#if WINDOWS_PHONE_APP
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif

            App.RootPage.NavigationFrame.Navigated += NavigationFrame_Navigated;
            HomePageNavigated += NavigationService_HomePageNavigated;
        }
        
        public void Reset()
        {
            CurrentPage = VLCPage.None;
            currentFlyout = VLCPage.None;
        }

        public void BindSplitShellEvents()
        {
            App.SplitShell.FlyoutNavigated += SplitShell_FlyoutNavigated;
            App.SplitShell.FlyoutClosed += SplitShell_FlyoutClosed;
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
#if WINDOWS_PHONE_APP
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = GoBack_Specific();
        }
#endif

        public void ShowBackButtonIfCanGoBack()
        {
            //if (CanGoBack())
            //    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            //else
            //    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
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
                case VLCPage.MainPageMusic:
                    Locator.MusicLibraryVM.GoBack();
                    break;
                case VLCPage.MainPageVideo:
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
                case VLCPage.PlaylistPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.CurrentPlaylistPage:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.VideoPlayerPage:
                   
                    //if (currentFlyout == VLCPage.VideoPlayerOptionsPanel || 
                    //    currentFlyout == VLCPage.SubtitlesSettings ||
                    //    currentFlyout == VLCPage.AudioTracksSettings ||
                    //    currentFlyout == VLCPage.ChaptersSettings )
                    //    GoBack_HideFlyout();

                    Locator.MediaPlaybackViewModel.GoBack.Execute(null);
                    GoBack_Default();
                    break;
                case VLCPage.MusicPlayerPage:
                    GoBack_Default();
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
                case VLCPage.MiniPlayerView:
                    AppViewHelper.ResizeWindow(true);
                    GoBack_Default();
                    break;
                // Settings pages
                case VLCPage.SettingsPage:
                    GoBack_HideFlyout();
                    break;
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
                case VLCPage.SubtitlesSettings:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.ChaptersSettings:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.AudioTracksSettings:
                    GoBack_HideFlyout();
                    break;
                case VLCPage.TrackEditorPage:
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
            currentFlyout = VLCPage.None;
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

            if (currentFlyout != VLCPage.None)
            {
                Go(currentFlyout);
            }
        }

        public void Go(VLCPage desiredPage, object param = null)
        {
            if (!IsFlyout(desiredPage) && desiredPage == CurrentPage) return;

            switch (desiredPage)
            {
                case VLCPage.MainPageXBOX:
                    setFlyoutContent(desiredPage, typeof(MainPageXBOX), param);
                    break;
                case VLCPage.MainPageVideo:
                case VLCPage.MainPageMusic:
                case VLCPage.MainPageFileExplorer:
                case VLCPage.MainPageNetwork:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(HomePage))
                        App.ApplicationFrame.Navigate(typeof(HomePage));
                    HomePageNavigated?.Invoke(null, desiredPage);
                    break;
                case VLCPage.AlbumPage:
                    setFlyoutContent(desiredPage, typeof(AlbumPageBase), param);
                    break;
                case VLCPage.SearchPage:
                    setFlyoutContent(desiredPage, typeof(SearchPage), param);
                    break;
                case VLCPage.SettingsPage:
                    setFlyoutContent(desiredPage, typeof(SettingsPage), param);
                    break;
                case VLCPage.SettingsPageUI:
                    setFlyoutContent(desiredPage, typeof(SettingsPageUI), param);
                    break;
                case VLCPage.SettingsPageMusic:
                    setFlyoutContent(desiredPage, typeof(SettingsPageMusic), param);
                    break;
                case VLCPage.SettingsPageVideo:
                    setFlyoutContent(desiredPage, typeof(SettingsPageVideo), param);
                    break;
                case VLCPage.PlaylistPage:
                    setFlyoutContent(desiredPage, typeof(PlaylistPage), param);
                    break;
                case VLCPage.CurrentPlaylistPage:
                    setFlyoutContent(desiredPage, typeof(MusicPlaylistPage), param);
                    break;
                case VLCPage.VideoPlayerPage:
                    App.ApplicationFrame.Navigate(typeof(VideoPlayerPage));
                    break;
                case VLCPage.MusicPlayerPage:
                    App.ApplicationFrame.Navigate(typeof(MusicPlayerPage));
                    break;
                case VLCPage.AddAlbumToPlaylistDialog:
                    setFlyoutContent(desiredPage, typeof(AddAlbumToPlaylistBase), param);
                    break;
                case VLCPage.CreateNewPlaylistDialog:
                    setFlyoutContent(desiredPage, typeof(CreateNewPlaylist), param);
                    break;
                case VLCPage.MiniPlayerView:
                    App.ApplicationFrame.Navigate(typeof(MiniPlayerWindow));
                    break;
                case VLCPage.VideoPlayerOptionsPanel:
                    setFlyoutContent(desiredPage, typeof(VideoPlayerOptionsPanel), param);
                    break;
                case VLCPage.SubtitlesSettings:
                    setFlyoutContent(desiredPage, typeof(SubtitlesSettings), param);
                    break;
                case VLCPage.AudioTracksSettings:
                    setFlyoutContent(desiredPage, typeof(AudioTracksSettings), param);
                    break;
                //case VLCPage.ChaptersSettings:
                //    setFlyoutContent(desiredPage, typeof(ChaptersSettings), param);
                //    break;
                case VLCPage.TrackEditorPage:
                    setFlyoutContent(desiredPage, typeof(TrackEditorPage), param);
                    break;
                case VLCPage.TvShowView:
                    setFlyoutContent(desiredPage, typeof(ShowEpisodesView), param);
                    break;
                case VLCPage.AboutAppView:
                    setFlyoutContent(desiredPage, typeof(LicensePage), param);
                    break;
                case VLCPage.ArtistShowsPage:
                    setFlyoutContent(desiredPage, typeof(ArtistPageBase), param);
                    break;
                //case VLCPage.ExternalStorageInclude:
                //    setFlyoutContent(desiredPage, typeof(ExternalStorageIncludePage), param);
                //    break;
                default:
                    break;
            }

            if (App.SplitShell.IsFlyoutOpen
                && !IsFlyout(desiredPage))
                GoBack_HideFlyout();
        }

        private async void onPlaybackStarted()
        {
            if (_playbackStartedTarget == null)
                return;
            await DispatchHelper.InvokeInUIThread(CoreDispatcherPriority.Normal, () =>
            {
                Go(_playbackStartedTarget.Value, _playbackStartedParam);
                _playbackStartedTarget = null;
                _playbackStartedParam = null;
            });
            Locator.PlaybackService.Playback_Opening -= onPlaybackStarted;
        }

        public void GoOnPlaybackStarted(VLCPage desiredPage, object param = null)
        {
            _playbackStartedTarget = desiredPage;
            _playbackStartedParam = param;
            Locator.PlaybackService.Playback_Opening += onPlaybackStarted;
        }

        private void setFlyoutContent(VLCPage desiredPage, Type t, object param)
        {
            // Do not switch the current flyout if it is modal.
            if (App.SplitShell.IsCurrentFlyoutModal())
                return;

            currentFlyout = desiredPage;
            App.SplitShell.SetFlyoutContentPresenter(t, param);
        }

        public bool IsFlyout(VLCPage page)
        {
            return page == VLCPage.AlbumPage ||
                   page == VLCPage.AddAlbumToPlaylistDialog ||
                   page == VLCPage.CreateNewPlaylistDialog ||
                   page == VLCPage.ArtistShowsPage ||
                   page == VLCPage.PlaylistPage ||
                   page == VLCPage.CurrentPlaylistPage ||
                   page == VLCPage.SettingsPageUI ||
                   page == VLCPage.SettingsPageMusic ||
                   page == VLCPage.SettingsPageVideo ||
                   page == VLCPage.SettingsPage ||
                   page == VLCPage.VideoPlayerOptionsPanel ||
                   page == VLCPage.SubtitlesSettings ||
                   page == VLCPage.AudioTracksSettings ||
                   page == VLCPage.ChaptersSettings ||
                   page == VLCPage.TvShowView ||
                   page == VLCPage.TrackEditorPage ||
                   page == VLCPage.AboutAppView ||
                   page == VLCPage.SearchPage ||
                   page == VLCPage.MainPageXBOX ||
                   page == VLCPage.ExternalStorageInclude;
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
            if (page == typeof(MiniPlayerWindow))
                return VLCPage.MiniPlayerView;
            return VLCPage.None;
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

        public void CloseVideoFlyouts()
        {
            if (currentFlyout == VLCPage.SubtitlesSettings || 
                currentFlyout == VLCPage.AudioTracksSettings ||
                currentFlyout == VLCPage.ChaptersSettings )
            {
                GoBack_HideFlyout();
            }
        }
    }
}
