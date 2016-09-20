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
using VLC.UI.Views.UserControls;

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
        private bool _preventAppExit = false;
        private string _informationText;
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

        public ActionCommand GoToFeedbackPageCommand { get; private set; } = new ActionCommand(() => Locator.NavigationService.Go(VLCPage.FeedbackPage));

        public ActionCommand GoToStreamPanel { get; private set; } = new ActionCommand(() => Locator.MainVM.CurrentPanel = Locator.MainVM.Panels.FirstOrDefault(x => x.Target == VLCPage.MainPageNetwork));

        public CreateMiniPlayerView CreateMiniPlayerView { get; private set; } = new CreateMiniPlayerView();

        public ScrollDetectedCommand ScrollDetectedCommand { get; private set; } = new ScrollDetectedCommand();

        public bool PreventAppExit
        {
            get { return _preventAppExit; }
            set { SetProperty(ref _preventAppExit, value); }
        }

        public string InformationText
        {
            get { return _informationText; }
            set { SetProperty(ref _informationText, value); }
        }

        public bool IsBackground
        {
            get { return _isBackground; }
            private set { SetProperty(ref _isBackground, value); }
        }

        public static bool IsWindows10 => (_isWindows10 ?? (_isWindows10 = IsWindows10Property())).Value;


        static bool IsWindows10Property()
        {
            // Based on an SO answer. Try to figure out if the device we are on supports both APIs.
            // If so, we are on 10. If not, we are on Desktop or Phone 8.1.
            var isWindows81 = Package.Current.GetType().GetRuntimeProperties().Any(r => r.Name == "DisplayName");
            var isWindowsPhone81 = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().GetType().GetRuntimeProperties().Any(r => r.Name == "RawPixelsPerViewPixel");

            var isWindows10 = isWindows81 && isWindowsPhone81;
            return isWindows10;
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
        }

        private async void ApplicationState_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == CoreWindowActivationState.Deactivated)
            {
                IsBackground = true;
                if (Locator.MediaPlaybackViewModel.CurrentMedia == null) return;
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
    }
}
