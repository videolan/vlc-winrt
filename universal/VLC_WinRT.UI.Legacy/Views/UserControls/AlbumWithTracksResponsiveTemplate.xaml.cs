using VLC_WinRT.Model.Video;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace VLC_WinRT.UI.Legacy.Views.UserControls
{
    public sealed partial class AlbumWithTracksResponsiveTemplate : UserControl
    {
        bool areTracksVisible = false;
        bool forceVisibleTracks = false;
        public AlbumWithTracksResponsiveTemplate()
        {
            this.InitializeComponent();
            this.Loaded += AlbumWithTracksResponsiveTemplate_Loaded;
        }

        private void AlbumWithTracksResponsiveTemplate_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.SizeChanged += AlbumWithTracksResponsiveTemplate_SizeChanged;
            ResponsiveTracksListView();
            Responsive();
        }

        private void AlbumWithTracksResponsiveTemplate_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            ResponsiveTracksListView();
            Responsive();
        }

        void Responsive()
        {
            if (forceVisibleTracks)
            {
                forceVisibleTracks = false;
                return;
            }
            
            if (this.ActualWidth > 600)
            {
                ShowTracks();
            }
            else
            {
                TracksListView.Visibility = PlayAppBarButton.Visibility = FavoriteAppBarButton.Visibility = PinAppBarButton.Visibility = Visibility.Collapsed;
                areTracksVisible = false;
            }

            if (this.ActualWidth > 900)
            {
                CoverImage.Width = CoverImage.Height = HeaderGrid.Height = 150;
            }
            else
            {
                CoverImage.Width = CoverImage.Height = HeaderGrid.Height = 90;
            }
        }

        void ShowTracks()
        {
            TracksListView.Visibility = PlayAppBarButton.Visibility = FavoriteAppBarButton.Visibility = PinAppBarButton.Visibility = Visibility.Visible;
            areTracksVisible = true;
        }

        void ResponsiveTracksListView()
        {
            var wrapGrid = TracksListView.ItemsPanelRoot as ItemsWrapGrid;
            if (wrapGrid == null) return;
            TemplateSizer.ComputeAlbumTracks(ref wrapGrid, TracksListView.ActualWidth - wrapGrid.Margin.Left - wrapGrid.Margin.Right);
        }

        private void HeaderGrid_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (!areTracksVisible)
            {
                forceVisibleTracks = true;
                ShowTracks();
            }
        }
    }
}
