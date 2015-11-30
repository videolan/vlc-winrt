using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Model.Video;

namespace VLC_WinRT.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistPageBase : Page
    {
        public ArtistPageBase()
        {
            this.InitializeComponent();
            this.Loaded += ArtistAlbumsList_Loaded;
        }

        private void ArtistAlbumsList_Loaded(object sender, RoutedEventArgs e)
        {
            App.SplitShell.ContentSizeChanged += SplitShell_ContentSizeChanged;
            AlbumsListView.SizeChanged += AlbumsListViewOnSizeChanged;
            this.Unloaded += ArtistAlbumsList_Unloaded;
            Responsive();
            ResponsiveListView();
        }

        private void AlbumsListViewOnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            ResponsiveListView();
        }

        private void SplitShell_ContentSizeChanged(double newWidth)
        {
            Responsive();
        }

        private void ArtistAlbumsList_Unloaded(object sender, RoutedEventArgs e)
        {
            App.SplitShell.ContentSizeChanged -= SplitShell_ContentSizeChanged;
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

        void ResponsiveListView()
        {

        }
    }
}
