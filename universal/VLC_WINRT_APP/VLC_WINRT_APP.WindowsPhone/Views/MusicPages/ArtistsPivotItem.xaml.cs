using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class ArtistsPivotItem : UserControl
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

        private void SemanticZoom_OnViewChangeCompletedCurrentArtistAlbum(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (e.IsSourceZoomedInView == false)
            {
                e.DestinationItem.Item = e.SourceItem.Item;
            }
        }
    }
}
