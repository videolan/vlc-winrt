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
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AppBarHelper.UpdateAppBar(typeof(MainPageHome));
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

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }


        private void Responsive()
        {
            MainHub.Orientation = Window.Current.Bounds.Width < 700 ? Orientation.Vertical : Orientation.Horizontal;
        }
    }
}
