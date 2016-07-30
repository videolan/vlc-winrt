using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Helpers;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class SongsPivotItem : Page
    {
        private bool isWide;
        public SongsPivotItem()
        {
            this.InitializeComponent();
        }

        private void Collection_Loaded(object sender, RoutedEventArgs e)
        {
            Locator.MusicLibraryVM.OnNavigatedToTracks();
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += OnUnloaded;
            Responsive();
        }

        void Current_SizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            Responsive();
        }


        void Responsive()
        {
            if (Window.Current.Bounds.Width > 500)
            {
                if (isWide) return;
                isWide = true;
                VisualStateUtilities.GoToState(this, "Wide", false);
            }
            else
            {
                if (!isWide) return;
                isWide = false;
                VisualStateUtilities.GoToState(this, "Narrow", false);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            if (TracksZoomedOutView.ItemsSource == null)
                TracksZoomedOutView.ItemsSource = GroupTracks.View.CollectionGroups;
        }

        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
        }

        private void ItemsWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive(sender as ItemsWrapGrid);
        }

        void Responsive(ItemsWrapGrid grid)
        {
            if (grid == null) return;
            double width;
            width = DisplayHelper.IsPortrait() ? Window.Current.Bounds.Width : 400;
            grid.ItemWidth = (width - 48) / 4;
            grid.ItemHeight = grid.ItemWidth;
        }
    }
}
