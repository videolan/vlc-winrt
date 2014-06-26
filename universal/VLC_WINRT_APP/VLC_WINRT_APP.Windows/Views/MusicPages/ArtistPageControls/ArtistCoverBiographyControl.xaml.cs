using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistCoverBiographyControl : UserControl
    {
        public ArtistCoverBiographyControl()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Window.Current.SizeChanged += CurrentOnSizeChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine("Unloading panel ArtistCoverBiographyControl");
            Window.Current.SizeChanged -= CurrentOnSizeChanged;
        }

        private void CurrentOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 400)
            {
                this.Width = Window.Current.Bounds.Width - 50;
            }
            else if (Window.Current.Bounds.Width < 800)
            {
                this.Width = Window.Current.Bounds.Width - 200;
            }
            else
            {
                this.Width = 768;
            }
        }
    }
}
