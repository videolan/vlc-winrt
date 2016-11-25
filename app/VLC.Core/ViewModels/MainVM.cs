/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Autofac;
using VLC.Commands.Navigation;
using VLC.Helpers;
using VLC.Commands;
using VLC.Model.Search;
using VLC.Services.RunTime;
using Panel = VLC.Model.Panel;
using Windows.UI.Popups;
using VLC.Model;
using libVLCX;
using VLC.Utils;
using WinRTXamlToolkit.Controls.Extensions;
using VLC.UI.Views.UserControls;
using Windows.UI.Xaml;
using VLC.UI.Views.MusicPages;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls.Primitives;

namespace VLC.ViewModels
{
    public class MainVM : BindableBase
    {
        #region private fields
        private ObservableCollection<Panel> _panels = new ObservableCollection<Panel>();
        #endregion
        #region private props
        private KeyboardListenerService keyboardListenerService;
        private Panel _currentPanel;
        private bool _isBackground = false;
        static bool? _isWindows10;

        #endregion

        #region public props
        public Panel CurrentPanel
        {
            get { return _currentPanel; }
            set
            {
                SetProperty(ref _currentPanel, value);
                Locator.NavigationService.Go(value.Target);
            }
        }

        public KeyboardListenerService KeyboardListenerService { get { return keyboardListenerService; } }


        public GoBackCommand GoBackCommand { get; private set; } = new GoBackCommand();
        public ActionCommand GoToHomePageMediaCenterCommand { get; private set; } = new ActionCommand(() => Locator.NavigationService.Go(VLCPage.MainPageXBOX));
        public ActionCommand GoToAboutViewCommand { get; private set; } = new ActionCommand(() => Locator.NavigationService.Go(VLCPage.AboutAppView));

        public ActionCommand GoToSettingsPageCommand { get; private set; } = new ActionCommand(() => Locator.NavigationService.Go(VLCPage.SettingsPage));

        public ActionCommand GoToSearchPageCommand { get; private set; } = new ActionCommand(() => Locator.NavigationService.Go(VLCPage.SearchPage));

        public ActionCommand GoToStreamPanel { get; private set; } = new ActionCommand(() => Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == VLCPage.MainPageNetwork));

        public CreateMiniPlayerView CreateMiniPlayerView { get; private set; } = new CreateMiniPlayerView();

        public ScrollDetectedCommand ScrollDetectedCommand { get; private set; } = new ScrollDetectedCommand();

        public AppBarClosedDisplayMode CommandBarDisplayMode
        {
            get
            {
                if (Locator.NavigationService.CurrentPage == VLCPage.CurrentPlaylistPage ||
                       Locator.NavigationService.CurrentPage == VLCPage.MusicPlayerPage ||
                       Locator.NavigationService.CurrentPage == VLCPage.VideoPlayerPage ||
                       Locator.NavigationService.CurrentPage == VLCPage.MiniPlayerView)
                    return AppBarClosedDisplayMode.Hidden;
                return Locator.MediaPlaybackViewModel.MiniPlayerVisibility == Visibility.Visible ?
                    AppBarClosedDisplayMode.Compact : AppBarClosedDisplayMode.Minimal;
            }
        }

        public bool IsBackground
        {
            get { return _isBackground; }
            private set { SetProperty(ref _isBackground, value); }
        }

        #endregion

        public MainVM()
        {
            keyboardListenerService = App.Container.Resolve<KeyboardListenerService>();

            Panels.Add(new Panel(Strings.Videos, VLCPage.MainPageVideo, App.Current.Resources["VideoSymbol"].ToString(), App.Current.Resources["VideoFilledSymbol"].ToString()));
            Panels.Add(new Panel(Strings.Music, VLCPage.MainPageMusic, App.Current.Resources["WaveSymbol"].ToString(), App.Current.Resources["WaveFilledSymbol"].ToString()));
            Panels.Add(new Panel(Strings.FileExplorer, VLCPage.MainPageFileExplorer, App.Current.Resources["FileExplorerSymbol"].ToString(), App.Current.Resources["FileExplorerFilledSymbol"].ToString()));
            Panels.Add(new Panel(Strings.Network, VLCPage.MainPageNetwork, App.Current.Resources["StreamSymbol"].ToString(), App.Current.Resources["StreamFilledSymbol"].ToString()));

            CoreWindow.GetForCurrentThread().Activated += ApplicationState_Activated;

            // The command bar display depends on:
            // - The current page
            Locator.NavigationService.ViewNavigated += (_, __) => NotifyCommandBarDisplayModeChanged();
            // And whether the media is a video or not
            Locator.PlaybackService.Playback_MediaSet += (_) => NotifyCommandBarDisplayModeChanged();
        }

        private async void NotifyCommandBarDisplayModeChanged()
        {
            await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Normal, () =>
            {
                OnPropertyChanged(nameof(CommandBarDisplayMode));
            });
        }

        private void ApplicationState_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                IsBackground = true;
                if (!Locator.MediaPlaybackViewModel.IsPlaying) return;
                
                // If we're playing a video, just pause.
                if (Locator.MediaPlaybackViewModel.PlaybackService.PlayingType == PlayingType.Video)
                {
                    // TODO: Route Video Player calls through Media Service
                    if (!Locator.SettingsVM.ContinueVideoPlaybackInBackground)
                    {
                        Locator.MediaPlaybackViewModel.PlaybackService.Pause();
                    }
                }
            }
            else
            {
                IsBackground = false;
            }
        }
        
        public ObservableCollection<Panel> Panels
        {
            get { return _panels; }
            set { SetProperty(ref _panels, value); }
        }

        public Visibility CollapsedInMediaCenterMode
        {
            get { return Locator.SettingsVM.MediaCenterMode ?
                    Visibility.Collapsed : Visibility.Visible; }
        }

        public Visibility CollapsedOnXbox
        {
            get
            {
                return DeviceHelper.GetDeviceType() == DeviceTypeEnum.Xbox ?
                    Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility VisibleOnXbox
        {
            get
            {
                return DeviceHelper.GetDeviceType() != DeviceTypeEnum.Xbox ?
                    Visibility.Collapsed : Visibility.Visible;
            }
        }

        public FlyoutPlacementMode FlyoutPlacement
        {
            get { return Locator.SettingsVM.DesktopMode ?
                    FlyoutPlacementMode.Top : FlyoutPlacementMode.Full; }
        }
    }
}
