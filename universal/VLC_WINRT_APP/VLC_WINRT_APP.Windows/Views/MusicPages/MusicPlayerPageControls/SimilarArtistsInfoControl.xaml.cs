using System.Diagnostics;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.MusicPages.MusicPlayerPageControls
{
    public sealed partial class SimilarArtistsInfoControl : UserControl
    {
        public SimilarArtistsInfoControl()
        {
            this.InitializeComponent();
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Window.Current.SizeChanged += CurrentOnSizeChanged;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Debug.WriteLine("Unloading panel SimilarArtistsInfoControl");
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
            (SimilarItemGridView.ItemsPanelRoot as WrapGrid).Orientation = (Window.Current.Bounds.Width < 1080)
                ? Orientation.Horizontal
                : Orientation.Vertical;
            if (Window.Current.Bounds.Width < 400)
            {
                (SimilarItemGridView.ItemsPanelRoot as WrapGrid).ItemHeight = 120;
                (SimilarItemGridView.ItemsPanelRoot as WrapGrid).ItemWidth = 120;
            }
            else if (Window.Current.Bounds.Width < 800)
            {
                (SimilarItemGridView.ItemsPanelRoot as WrapGrid).ItemHeight = 160;
                (SimilarItemGridView.ItemsPanelRoot as WrapGrid).ItemWidth = 160;
            }
            else
            {
                (SimilarItemGridView.ItemsPanelRoot as WrapGrid).ItemHeight = 185;
                (SimilarItemGridView.ItemsPanelRoot as WrapGrid).ItemWidth = 185;
            }
        }
    }
}
