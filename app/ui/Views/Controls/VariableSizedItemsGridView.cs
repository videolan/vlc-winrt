/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Xaml.Controls;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Views.Controls
{
    public class VariableSizedItemsGridView : GridView
    {
        private int i = -1;
        
        protected override void PrepareContainerForItemOverride(Windows.UI.Xaml.DependencyObject element, object item)
        {
            i++;
            MediaViewModel MediaViewModel;
            MediaViewModel viewedVideo;
            MusicLibraryViewModel.AlbumItem albumItem;
            MediaViewModel = item as MediaViewModel;
            viewedVideo = item as MediaViewModel;
            albumItem = item as VLC_WINRT.ViewModels.MainPage.MusicLibraryViewModel.AlbumItem;

            if (viewedVideo != null)
            {
                if (viewedVideo.Duration.TotalSeconds > viewedVideo.TimeWatched.TotalSeconds / 2 && i == 0)
                {
                    element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 2);
                    element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 2);
                }
            }
            else if (MediaViewModel != null || albumItem != null)
            {
                if (i == 0)
                {
                    element.SetValue(VariableSizedWrapGrid.ColumnSpanProperty, 2);
                    element.SetValue(VariableSizedWrapGrid.RowSpanProperty, 2);
                }
            }
            base.PrepareContainerForItemOverride(element, item);
        }
    }
}
