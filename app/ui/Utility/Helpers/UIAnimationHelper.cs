/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace VLC_WINRT.Utility.Helpers
{
    public class UIAnimationHelper
    {
        #region FadeIn()
        public static void FadeIn(UIElement element)
        {
            element.Visibility = Visibility.Visible;
            var fadeInStoryboard = new Storyboard();
            var fadeInAnimation = new FadeInThemeAnimation();

            Storyboard.SetTarget(fadeInAnimation, element);
            fadeInStoryboard.Children.Add(fadeInAnimation);
            fadeInStoryboard.Begin();
        }
        #endregion

        #region FadeOut()
        public static void FadeOut(UIElement element)
        {
            var fadeOutStoryboard = new Storyboard();
            var fadeOutAnimation = new FadeOutThemeAnimation();

            Storyboard.SetTarget(fadeOutAnimation, element);
            fadeOutStoryboard.Children.Add(fadeOutAnimation);
            fadeOutStoryboard.Completed += (sender, o) => element.Visibility = Visibility.Collapsed;
            fadeOutStoryboard.Begin();
        }
        #endregion
    }
}
