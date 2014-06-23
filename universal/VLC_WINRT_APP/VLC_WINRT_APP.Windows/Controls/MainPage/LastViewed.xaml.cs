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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT_APP.ViewModels.MainPage;

namespace VLC_WINRT.Views.Controls.MainPage
{
    public sealed partial class LastViewed : UserControl
    {
        public LastViewed()
        {
            InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            //if (Window.Current.Bounds.Width == 320)
            //{
                //SnapGrid.Visibility = Visibility.Visible;
                //HorizontalScrollViewer.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
                SnapGrid.Visibility = Visibility.Collapsed;
                HorizontalScrollViewer.Visibility = Visibility.Visible;
            //}
        }

        private void FavoriteAlbumItemClick(object sender, ItemClickEventArgs e)
        {
            (e.ClickedItem as MusicLibraryViewModel.AlbumItem).PlayAlbum.Execute(e.ClickedItem);
        }
    }
}
