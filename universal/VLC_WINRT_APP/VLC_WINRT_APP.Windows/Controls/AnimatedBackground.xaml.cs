/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Linq;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WINRT_APP.Utility.Helpers;
using VLC_WINRT.ViewModels;
using VLC_WINRT.Common;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT.Views.Controls
{
    public sealed partial class AnimatedBackground : UserControl
    {
        ThreadPoolTimer _periodicTimer;
        private bool _isInit;
        public AnimatedBackground()
        {
            this.InitializeComponent();
            if(!_isInit)
                SetSourceToImages();
        }

        void SetSourceToImages()
        {
            _isInit = true;
            FirstImage.ImageOpened += ImageOpened;
            TimeSpan period = TimeSpan.FromSeconds(25);
            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                int i = new Random().Next(0, Locator.MusicLibraryVM.ImgCollection.Count);
                await DispatchHelper.InvokeAsync(() =>
                    {
                        if (!Locator.MusicLibraryVM.ImgCollection.Any()) return;
                        FirstImage.Source = new BitmapImage(new Uri(Locator.MusicLibraryVM.ImgCollection[i], UriKind.RelativeOrAbsolute));
                        
                        ZoomAnimation1.Begin();
                    });
            }, period);
        }

        private void ImageOpened(object sender, RoutedEventArgs routedEventArgs)
        {
            UIAnimationHelper.FadeIn(FirstImage);
        }
    }
}
