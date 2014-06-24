/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using VLC_WINRT_APP.Helpers;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace VLC_WINRT.Views.Controls.MusicPage
{
    public sealed partial class AlbumFlyout : UserControl
    {
        public AlbumFlyout()
        {
            this.InitializeComponent();
            UIAnimationHelper.FadeOut(this);
        }
        public void Show()
        {
            UIAnimationHelper.FadeIn(this);
        }
        public void Hide()
        {
            UIAnimationHelper.FadeOut(this);
        }
    }
}
