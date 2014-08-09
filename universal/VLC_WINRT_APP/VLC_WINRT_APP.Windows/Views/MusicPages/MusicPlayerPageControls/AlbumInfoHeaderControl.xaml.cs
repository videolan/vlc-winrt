using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.MusicPages.MusicPlayerPageControls
{
    public sealed partial class AlbumInfoHeaderControl : UserControl
    {
        public AlbumInfoHeaderControl()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += CurrentOnSizeChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine("Unloading panel AlbumInfoHeaderControl");
            Window.Current.SizeChanged -= CurrentOnSizeChanged;
        }
        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Responsive();
        }

        private void CurrentOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width < 550)
            {
                CoverImage.Height = 130;
                CoverImage.Width = 130;
                NoCoverImage.Height = 130;
                NoCoverImage.Width = 130;
                CoverImageColumnDefinition.Width = new GridLength(130);
                PlayAllAppBarToggleButton.Margin = new Thickness(-27, 0, 0, 0);
            }
            else
            {
                CoverImage.Height = 200;
                CoverImage.Width = 200;
                NoCoverImage.Height = 200;
                NoCoverImage.Width = 200;
                CoverImageColumnDefinition.Width = new GridLength(200);
                PlayAllAppBarToggleButton.Margin = new Thickness(0);
            }
        }
    }
}
