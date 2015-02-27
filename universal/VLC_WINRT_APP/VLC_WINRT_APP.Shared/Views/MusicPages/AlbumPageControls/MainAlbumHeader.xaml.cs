using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT_APP.Views.MusicPages.AlbumPageControls
{
    public sealed partial class MainAlbumHeader : UserControl
    {
        public MainAlbumHeader()
        {
            this.InitializeComponent();
#if WINDOWS_APP
            CoverColumnDefinition.MinWidth = 200;
#endif
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged += OnSizeChanged;
            this.Unloaded += OnUnloaded;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }
    }
}
