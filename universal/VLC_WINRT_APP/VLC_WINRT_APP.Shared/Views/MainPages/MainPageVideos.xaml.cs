/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageVideos : Page
    {
        public MainPageVideos()
        {
            InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AppBarHelper.UpdateAppBar(typeof(MainPageVideos));
            if (e.NavigationMode == NavigationMode.New && Locator.VideoLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.VideoLibraryVM.Initialize();
            }
        }

        private void MainPageVideoContentPresenter_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (MainPageVideoContentPresenter.CurrentSourcePageType == null)
            {
                Locator.MainVM.ChangeMainPageVideoViewCommand.Execute((int) Locator.SettingsVM.VideoView);
            }
#if WINDOWS_APP
            MainPageVideoContentPresenter.Margin = new Thickness(24, 0, 24, 0);
            MainPageVideoContentPresenter.ContentTransitions = new TransitionCollection()
            {
                new EdgeUIThemeTransition() {Edge = EdgeTransitionLocation.Right},
            };
#else
            MainPageVideoContentPresenter.ContentTransitions = new TransitionCollection()
            {
                new NavigationThemeTransition()
                {
                    DefaultNavigationTransitionInfo = new CommonNavigationTransitionInfo(),
                }
            };
            MainPageVideoContentPresenter.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
#endif
        }
    }
}
