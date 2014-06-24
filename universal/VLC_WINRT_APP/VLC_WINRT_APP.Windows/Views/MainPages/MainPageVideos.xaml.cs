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
using VLC_WINRT_APP.Helpers;
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
            this.SizeChanged += OnSizeChanged;
        }


        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            Locator.VideoLibraryVM.ExecuteSemanticZoom(sender as SemanticZoom, VideoGroupedByAlphaKey);
        }

        private void Panels_OnItemClick(object sender, ItemClickEventArgs e)
        {
            Model.Panel panel = e.ClickedItem as Model.Panel;
            foreach (var panel1 in Locator.VideoLibraryVM.Panels)
            {
                panel1.Opacity = 0.4;
            }
            panel.Opacity = 1;
            switch (panel.Title)
            {
                case "all":
                    SemanticZoom.Visibility = Visibility.Visible;
                    NewVideosListView.Visibility = Visibility.Collapsed;
                    break;
                case "new":
                    SemanticZoom.Visibility = Visibility.Collapsed;
                    NewVideosListView.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
