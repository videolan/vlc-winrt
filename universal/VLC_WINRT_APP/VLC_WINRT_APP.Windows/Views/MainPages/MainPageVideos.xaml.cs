/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageVideos : Page
    {
        public MainPageVideos()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && Locator.VideoLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Locator.VideoLibraryVM.Initialize();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (Window.Current.Bounds.Width < 400)
            {
                RootGrid.Margin = new Thickness(9, 0, 0, 0);
                FirstRowDefinition.Height = new GridLength(0);
            }
            else
            {
                RootGrid.Margin = new Thickness(40, 0, 0, 0);
                FirstRowDefinition.Height = new GridLength(42);
            }
        }

        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
        }

        private void ItemsWrapGrid_Loaded(object sender, SizeChangedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
        }

        void Responsive(ItemsWrapGrid itemsWrap)
        {
            int usefulWidth = (int)Window.Current.Bounds.Width;
            Debug.WriteLine(usefulWidth);
            int sidebar;
            if (Window.Current.Bounds.Width < 400)
            {
                sidebar = 30;
            }
            else if (Window.Current.Bounds.Width < 1080)
            {
                sidebar = 140;
            }
            else
            {
                sidebar = 400;
            }
            usefulWidth -= sidebar;

            if (usefulWidth < 400)
            {
                itemsWrap.ItemWidth = usefulWidth;
                itemsWrap.ItemHeight = usefulWidth * 0.561;
            }
            else if (usefulWidth < 890)
            {
                itemsWrap.ItemWidth = usefulWidth / 2;
                itemsWrap.ItemHeight = (usefulWidth / 2) * 0.561;
            }
            else if (usefulWidth < 1300)
            {
                itemsWrap.ItemWidth = usefulWidth / 3;
                itemsWrap.ItemHeight = (usefulWidth / 3) * 0.561;
            }
            else
            {
                itemsWrap.ItemWidth = usefulWidth / 4;
                itemsWrap.ItemHeight = (usefulWidth / 4) * 0.561;
            }
        }
    }
}
