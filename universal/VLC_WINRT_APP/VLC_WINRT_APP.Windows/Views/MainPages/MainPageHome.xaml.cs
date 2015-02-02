/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Diagnostics;
using System.Threading.Tasks;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Video;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageHome : Page
    {
        public MainPageHome()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }


        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AppBarHelper.UpdateAppBar(typeof (MainPageHome));
            if (Locator.VideoLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                Task.Run(() => Locator.VideoLibraryVM.Initialize());
            }
            if (Locator.MusicLibraryVM.LoadingState == LoadingState.NotLoaded)
            {
                // Do not initialize all musiclibrary
                // We only need some favorite albums, and random albums. Check them from the SQL database
                Task.Run(() => Locator.MusicLibraryVM.Initialize());
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void Responsive()
        {
            if (Window.Current.Bounds.Width < 600)
            {
                MainHub.Orientation = Orientation.Vertical;
            }
            else
            {
                MainHub.Orientation = Orientation.Horizontal;
            }
        }

        private void VideoItemWrapGrid_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
            var wrapGrid = sender as ItemsWrapGrid;
            TemplateSizer.ComputeCompactVideo(wrapGrid);
        }

        private void Responsive(ItemsWrapGrid itemsWrap)
        {
            if (Window.Current.Bounds.Width < 600)
            {
                itemsWrap.Orientation = Orientation.Horizontal;
            }
            else
            {
                itemsWrap.Orientation = Orientation.Vertical;
            }
        }

        private void AlbumsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid);
        }
    }
}
