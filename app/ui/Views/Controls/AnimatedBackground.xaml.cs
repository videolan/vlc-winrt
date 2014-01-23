using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Pour en savoir plus sur le modèle d'élément Contrôle utilisateur, consultez la page http://go.microsoft.com/fwlink/?LinkId=234236
using VLC_WINRT.Utility.Helpers;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Views.Controls
{
    public sealed partial class AnimatedBackground : UserControl
    {
        ThreadPoolTimer _periodicTimer;
        public AnimatedBackground()
        {
            this.InitializeComponent();
            SetSourceToImages();
        }

        void SetSourceToImages()
        {
            TimeSpan period = TimeSpan.FromSeconds(25);
            _periodicTimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                int i = new Random().Next(0, Locator.MusicLibraryVM.ImgCollection.Count);
                App.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        FirstImage.Source = new BitmapImage(new Uri(Locator.MusicLibraryVM.ImgCollection[i], UriKind.RelativeOrAbsolute));
                        ZoomAnimation1.Begin();
                    });

            }, period);
        }
    }
}
