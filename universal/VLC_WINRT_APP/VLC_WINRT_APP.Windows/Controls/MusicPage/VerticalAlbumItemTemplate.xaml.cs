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
using Windows.UI.Xaml.Input;

// Pour en savoir plus sur le modèle d'élément Contrôle utilisateur, consultez la page http://go.microsoft.com/fwlink/?LinkId=234236
using VLC_WINRT_APP.Helpers;

namespace VLC_WINRT.Views.Controls.MusicPage
{
    public sealed partial class VerticalAlbumItemTemplate : UserControl
    {
        public VerticalAlbumItemTemplate()
        {
            this.InitializeComponent();
            UIAnimationHelper.FadeOut(PlayListGrid);
        }

        private void Album_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (PlayListGrid.Visibility == Visibility.Visible)
            {
                ClosePlayListArrow.Visibility = Visibility.Collapsed;
                OpenPlayListArrow.Visibility = Visibility.Visible;
                UIAnimationHelper.FadeOut(PlayListGrid);
            }
            else
            {
                OpenPlayListArrow.Visibility = Visibility.Collapsed;
                ClosePlayListArrow.Visibility = Visibility.Visible;
                UIAnimationHelper.FadeIn(PlayListGrid);
            }
        }
    }
}
