using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.Model.Video;

namespace VLC_WINRT_APP.Views.MainPages.MainMusicControls
{
    public sealed partial class ArtistsPivotItem : Page
    {
        public ArtistsPivotItem()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Unloaded += OnUnloaded;
            this.SizeChanged += OnSizeChanged;
            Responsive();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }

        void Responsive()
        {
        }

        private void ArtistsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid);
        }
    }
}
