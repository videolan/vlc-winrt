using System;
using System.Linq;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.ViewModels;

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
            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                int i = new Random().Next(0, Locator.MusicLibraryVM.ImgCollection.Count);
                App.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
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
