/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/
using Windows.Graphics.Display;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Autofac;
using VLC_WinRT.Helpers;
using VLC_WinRT.Services.RunTime;
using VLC_WinRT.ViewModels;
using Windows.UI.ViewManagement;
using Slide2D;
using Windows.UI.Xaml.Navigation;
using VLC_WinRT.Utils;
using VLC_WinRT.Model;
using System;
using System.Linq;
using Windows.System;
using System.Diagnostics;
using Windows.UI.Composition;
using System.Numerics;
using Windows.UI.Xaml.Hosting;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
#if WINDOWS_APP
using Windows.UI.ApplicationSettings;
#endif

namespace VLC_WinRT.Views.MainPages
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
            var smtc = SystemMediaTransportControls.GetForCurrentView();
            Locator.MediaPlaybackViewModel.SetMediaTransportControls(smtc);
            this.GotFocus += MainPage_GotFocus;
#if WINDOWS_UWP
            this.Loaded += MainPage_Loaded;
#endif
        }

        private void MainPage_GotFocus(object sender, RoutedEventArgs e)
        {
            var el = e.OriginalSource as FrameworkElement;
           
            var output ="GOTFOCUS --" + e.OriginalSource.ToString() + "--" + el.Name + el.BaseUri;
            DebugString(output);

            Debug.WriteLine(output);
        }

        public void SetBackground(bool force = false, bool dark = false)
        {
            if (force)
            {
                if (dark)
                    Dark.Begin();
                else
                    Light.Begin();
            }
            else
            {
                if (Locator.SettingsVM.ApplicationTheme == ApplicationTheme.Dark)
                    Dark.Begin();
                else
                    Light.Begin();
            }
        }
        
        private void SplitShell_FlyoutCloseRequested(object sender, System.EventArgs e)
        {
            Locator.NavigationService.GoBack_HideFlyout();
        }
        
        private void MainFrame_OnNavigated(object sender, NavigationEventArgs e)
        {
#if WINDOWS_APP
            SettingsPane pane = SettingsPane.GetForCurrentView();
            pane.CommandsRequested += SettingsCommandRequested;
#endif
        }

#if WINDOWS_APP
        private void SettingsCommandRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            var privacyCommand = new SettingsCommand("privacy", Strings.PrivacyStatement,
                async h => await Launcher.LaunchUriAsync(new Uri("http://videolan.org/vlc/privacy.html")));

            var specialThanks = new SettingsCommand("specialThanks", Strings.SpecialThanks,
                command =>
                {
                    Locator.NavigationService.Go(VLCPage.SpecialThanksPage);
                });

            var settings = new SettingsCommand("settings", Strings.Settings,
                command =>
                {
                    Locator.NavigationService.Go(VLCPage.SettingsPage);
                });
            var license = new SettingsCommand("license", Strings.License, command =>
            {
                Locator.NavigationService.Go(VLCPage.LicensePage);
            });
            args.Request.ApplicationCommands.Clear();
            args.Request.ApplicationCommands.Add(privacyCommand);
            args.Request.ApplicationCommands.Add(specialThanks);
            args.Request.ApplicationCommands.Add(settings);
            args.Request.ApplicationCommands.Add(license);
        }
#endif

        public void DebugString(string s)
        {
#if DEBUG
            DebugTextBlock.Text = s;
#endif
        }

#if WINDOWS_UWP

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationFrame.AllowDrop = true;
            NavigationFrame.DragOver += NavigationFrame_DragOver;
            NavigationFrame.Drop += NavigationFrame_Drop;

            SwapChainPanel.Tapped += SwapChainPanel_Tapped;
        }



        private async void NavigationFrame_Drop(object sender, DragEventArgs e)
        {
            var storageItems = await e.DataView.GetStorageItemsAsync();
            if (!storageItems.Any())
                return;
            await Locator.MediaPlaybackViewModel.OpenFile(storageItems[0] as Windows.Storage.StorageFile);
        }

        private void NavigationFrame_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            e.DragUIOverride.Caption = Strings.OpenFile;
            e.DragUIOverride.IsGlyphVisible = false;
        }

        private Compositor _compositor;
        private bool _pipEnabled;
        public void StartCompositionAnimationOnSwapChain(bool pipEnabled)
        {
            _pipEnabled = pipEnabled;
            SwapChainPanel.Visibility = Visibility.Visible;

            Canvas.SetZIndex(SwapChainPanel, _pipEnabled ? 1 : 0);

            var root = ElementCompositionPreview.GetElementVisual(RootGrid);
            _compositor = root.Compositor;
            Animate();
            SplitShell.ContentSizeChanged += (s) => Animate();
        }
        
        public async void StopCompositionAnimationOnSwapChain()
        {
            if (_compositor == null)
                return;

            _pipEnabled = false;
            var target = ElementCompositionPreview.GetElementVisual(SwapChainPanel);
            var opacityAnim = _compositor.CreateScalarKeyFrameAnimation();
            opacityAnim.InsertKeyFrame(1f, 0f);

            opacityAnim.Duration = TimeSpan.FromMilliseconds(500);
            opacityAnim.IterationCount = 1;
            target.StartAnimation(nameof(Visual.Opacity), opacityAnim);
            await Task.Delay((int)opacityAnim.Duration.TotalMilliseconds);
            SwapChainPanel.Visibility = Visibility.Collapsed;
        }

        void Animate()
        {
            var target = ElementCompositionPreview.GetElementVisual(SwapChainPanel);
            if (!_pipEnabled) // Don't needlessly call composition APIs
            {
                if (target.Offset.X == 0f && target.Offset.Y == 0f && target.Opacity != 1f)
                    return;
            }

            Locator.MediaPlaybackViewModel.PlaybackService.SetSizeVideoPlayer((uint)Math.Ceiling(App.RootPage.SwapChainPanel.ActualWidth), (uint)Math.Ceiling(App.RootPage.SwapChainPanel.ActualHeight));
            Locator.VideoPlayerVm.ChangeSurfaceZoom(Locator.VideoPlayerVm.CurrentSurfaceZoom);


            Debug.WriteLine($"swapWidth {SwapChainPanel.ActualWidth} -- winWidth {Window.Current.Bounds.Width}");
            
            // Position Animation
            var posAnimation = _compositor.CreateVector3KeyFrameAnimation();
            if (_pipEnabled)
            {
                posAnimation.InsertKeyFrame(1f, new Vector3((float)Window.Current.Bounds.Width * 0.7f, (float)Window.Current.Bounds.Height * 0.7f, 1f));
            }
            else
            {
                posAnimation.InsertKeyFrame(1f, new Vector3(0f, 0f, 1f));
            }

            // Opacity animation
            var opacityAnim = _compositor.CreateScalarKeyFrameAnimation();
            opacityAnim.InsertKeyFrame(1f, 1f);

            // Scale animation
            var scaleAnimation = _compositor.CreateVector3KeyFrameAnimation();
            if (_pipEnabled)
                scaleAnimation.InsertKeyFrame(1f, new Vector3(0.3f, 0.3f, 1f));
            else
                scaleAnimation.InsertKeyFrame(1f, new Vector3(1f, 1f, 1f));

            // Set animation properties
            opacityAnim.Duration = scaleAnimation.Duration = posAnimation.Duration = TimeSpan.FromMilliseconds(500);
            opacityAnim.IterationCount = scaleAnimation.IterationCount = posAnimation.IterationCount = 1;

            target.StartAnimation("Offset", posAnimation);
            target.StartAnimation("Scale", scaleAnimation);
            target.StartAnimation("Opacity", opacityAnim);
        }

        private void SwapChainPanel_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Locator.NavigationService.Go(VLCPage.VideoPlayerPage);
        }
#endif
    }
}