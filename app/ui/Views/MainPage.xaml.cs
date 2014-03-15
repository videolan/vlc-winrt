/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Diagnostics;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.Commands;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.Views.Controls.InputDialog;
using Panel = VLC_WINRT.Model.Panel;

namespace VLC_WINRT.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : BasePage
    {
        private int _currentSection;
        public MainPage()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
            this.Loaded += (sender, args) =>
            {
                FadeOutPage.Begin();
                ChangeLayout(Window.Current.Bounds.Width);
                for (int i = 0; i < SectionsGrid.Children.Count; i++)
                {
                    if (i == _currentSection) continue;
                    UIAnimationHelper.FadeOut(SectionsGrid.Children[i]);
                }
                FadeInPage.Begin();
                if (!string.IsNullOrEmpty(App.TemporaryMRL))
                {
                    OpenVideoFromFileExplorer();
                }
            };
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        public async void OpenVideoFromFileExplorer()
        {
            Debug.WriteLine("Opening file: " + App.TemporaryFileName);
            Locator.PlayVideoVM.SetActiveVideoInfo(App.TemporaryMRL, App.TemporaryFileName);
            NavigationService.NavigateTo(typeof(PlayVideo));
            App.TemporaryFileName = null;
            App.TemporaryMRL = null;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ChangeLayout(sizeChangedEventArgs.NewSize.Width);
        }

        void ChangeLayout(double x)
        {
            Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                {
                    if (x < 1050)
                    {
                        SecondarySectionsHeaderListView.Visibility = Visibility.Collapsed;
                        MorePanelsButton.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        SecondarySectionsHeaderListView.Visibility = Visibility.Visible;
                        MorePanelsButton.Visibility = Visibility.Collapsed;
                    }

                    if (x == 320)
                    {
                        HeaderGrid.Margin = new Thickness(0, 20, 0, 0);
                    }
                    else
                    {
                        HeaderGrid.Margin = new Thickness(50, 40, 0, 0);
                    }
                });
        }

        public override void SetDataContext()
        {
            _vm = (NavigateableViewModel)DataContext;
        }

        private void SectionsHeaderListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var i = ((Model.Panel)e.ClickedItem).Index;
            ChangedSectionsHeadersState(i);
        }

        public void ChangedSectionsHeadersState(int i)
        {
            if (i == _currentSection) 
                return;
            App.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UIAnimationHelper.FadeOut(SectionsGrid.Children[_currentSection]);
                UIAnimationHelper.FadeIn(SectionsGrid.Children[i]);
                _currentSection = i;
                for (int j = 0; j < SectionsHeaderListView.Items.Count; j++)
                    Locator.MainPageVM.Panels[j].Opacity = 0.4;
                Locator.MainPageVM.Panels[i].Opacity = 1;
            });
        }

        private async void MorePanelsButton_OnClick(object sender, RoutedEventArgs e)
        {
            CreateVLCMenu();
        }

        private void SecondarySectionsHeaderListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            int index = (e.ClickedItem as Panel).Index;
            switch (index)
            {
                case 3:
                    ExternalStorage();
                    break;
                case 4:
                    MediaServers();
                    break;
                case 5:
                    OpenVideo();
                    break;
                case 6:
                    OpenStream();
                    break;
            }
        }

        async void ExternalStorage()
        {
            await FadeOutPage.BeginAsync();
            NavigationService.NavigateTo(typeof(RemovableStoragePage));   
        }

        async void MediaServers()
        {
            await FadeOutPage.BeginAsync();
            NavigationService.NavigateTo(typeof(DLNAPage));
        }

        async void OpenVideo()
        {
            Locator.MainPageVM.PickVideo.Execute(null);
        }

        async void OpenStream()
        {
            var dialog = new InputDialog();
            RootGrid.Children.Add(dialog);
            Grid.SetRow(dialog, 1);
            // NOTE: Is this being used?
            await
                dialog.ShowAsync("", "Open a file from network",
                    "Please enter an address (Ex: FTP, HTTP).", "Open",
                    Locator.MainPageVM.PlayNetworkMRL);
        }

        public async void CreateVLCMenu()
        {
            var resourceLoader = new ResourceLoader();
            var popupMenu = new PopupMenu();
            popupMenu.Commands.Add(new UICommand(resourceLoader.GetString("PrivacyStatement"), async h => ExternalStorage()));

            popupMenu.Commands.Add(new UICommand("Media servers", async h => MediaServers()));


            var transform = RootGrid.TransformToVisual(this);
            var point = transform.TransformPoint(new Point(270, 110));
            await popupMenu.ShowAsync(point);
        }

        private void OpenSearchPane(object sender, RoutedEventArgs e)
        {
            App.RootPage.SearchPane.Show();
        }

        private async void OpenFile(object sender, RoutedEventArgs e)
        {
            var resourceLoader = new ResourceLoader();
            var popupMenu = new PopupMenu();
            popupMenu.Commands.Add(new UICommand(resourceLoader.GetString("OpenVideo"), async h => OpenVideo()));



            var transform = RootGrid.TransformToVisual(this);
            var point = transform.TransformPoint(new Point(Window.Current.Bounds.Width - 110, 200));
            await popupMenu.ShowAsync(point);
        }
    }
}
