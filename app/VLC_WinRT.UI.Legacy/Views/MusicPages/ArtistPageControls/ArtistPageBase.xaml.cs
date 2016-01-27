using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml.Navigation;

namespace VLC_WinRT.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistPageBase : Page
    {
        public ArtistPageBase()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            App.SplitShell.ContentSizeChanged += SplitShell_ContentSizeChanged;
            AlbumsListView.SizeChanged += AlbumsListViewOnSizeChanged;
            Responsive();
            ResponsiveTracksListView();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            App.SplitShell.ContentSizeChanged -= SplitShell_ContentSizeChanged;
        }

        private void AlbumsListViewOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ResponsiveTracksListView();
        }

        private void SplitShell_ContentSizeChanged(double newWidth)
        {
            Responsive();
        }
        
        void Responsive()
        {
            if (Window.Current.Bounds.Width < 1150)
            {
                VisualStateUtilities.GoToState(this, "Narrow", false);
            }
            else if (Window.Current.Bounds.Width < 1450)
            {
                VisualStateUtilities.GoToState(this, "Medium", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
            }
        }

        void ResponsiveTracksListView()
        {
            var wrapGrid = AlbumsListView.ItemsPanelRoot as ItemsWrapGrid;
            if (wrapGrid == null) return;
            TemplateSizer.ComputeAlbumTracks(ref wrapGrid, AlbumsListView.ActualWidth - wrapGrid.Margin.Left - wrapGrid.Margin.Right);
        }
    }
}
