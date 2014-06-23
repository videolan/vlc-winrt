/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Utility.Helpers;
using VLC_WINRT.ViewModels;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageVideos : Page
    {
        private int _currentSection;

        public MainPageVideos()
        {
            InitializeComponent();
            //this.Loaded += (sender, args) =>
            //{
            //    for (int i = 1; i < SectionsGrid.Children.Count; i++)
            //    {
            //        UIAnimationHelper.FadeOut(SectionsGrid.Children[i]);
            //    }
            //};
            this.SizeChanged += OnSizeChanged;
        }

        private async void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            await DispatchHelper.InvokeAsync(() =>
            {

                if (sizeChangedEventArgs.NewSize.Width == 320)
                {
                    SemanticZoomVertical.Visibility = Visibility.Visible;
                    SemanticZoom.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SemanticZoomVertical.Visibility = Visibility.Collapsed;
                    SemanticZoom.Visibility = Visibility.Visible;
                }
            });
        }
        private void SectionsHeaderListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            //var i = ((Model.Panel)e.ClickedItem).Index;
            //ChangedSectionsHeadersState(i);
        }
        private void ChangedSectionsHeadersState(int i)
        {
            if (i == _currentSection) return;
            UIAnimationHelper.FadeOut(SectionsGrid.Children[_currentSection]);
            UIAnimationHelper.FadeIn(SectionsGrid.Children[i]);
            _currentSection = i;
            //for (int j = 0; j < SectionsHeaderListView.Items.Count; j++)
            //    Locator.MainVM.VideoVM.Panels[j].Opacity = 0.4;
            Locator.VideoLibraryVM.Panels[i].Opacity = 1;
        }
        
        private void OnHeaderSemanticZoomClicked(object sender, RoutedEventArgs e)
        {
            SemanticZoomVertical.IsZoomedInViewActive = false;
            SemanticZoom.IsZoomedInViewActive = false;
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
        }
    }
}
