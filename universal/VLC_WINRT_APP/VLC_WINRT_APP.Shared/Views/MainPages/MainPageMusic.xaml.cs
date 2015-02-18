/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Linq;
using Windows.Devices.Input;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages.MusicPanes;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageMusic : Page
    {
        public MainPageMusic()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AppBarHelper.UpdateAppBar(typeof(MainPageMusic));
        }

        private void MusicPanesFrame_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (MainPageMusicContentPresenter.CurrentSourcePageType == null)
            {
                Locator.MainVM.ChangeMainPageMusicViewCommand.Execute((int)Locator.SettingsVM.MusicView);
            }
#if WINDOWS_APP
            MainPageMusicContentPresenter.ContentTransitions = new TransitionCollection()
            {
                new EdgeUIThemeTransition() {Edge = EdgeTransitionLocation.Right},
            };
#else
            MainPageMusicContentPresenter.ContentTransitions = new TransitionCollection()
            {
                new NavigationThemeTransition()
                {
                    DefaultNavigationTransitionInfo = new CommonNavigationTransitionInfo(),
                }
            };
            MainPageMusicContentPresenter.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
#endif
        }
    }
}
