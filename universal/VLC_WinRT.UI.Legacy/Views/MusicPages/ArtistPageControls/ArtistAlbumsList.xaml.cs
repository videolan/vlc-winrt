using System;
using Windows.UI.Core;
using VLC_WinRT.Model.Video;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace VLC_WinRT.Views.MusicPages.ArtistPageControls
{
    public sealed partial class ArtistAlbumsList : UserControl
    {
        public ArtistAlbumsList()
        {
            this.InitializeComponent();
            this.Loaded += ArtistAlbumsList_Loaded;
        }

        private void ArtistAlbumsList_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += CurrentOnSizeChanged;
            this.Unloaded += ArtistAlbumsList_Unloaded;
            Responsive();
            AlbumsZoomedOutView.ItemsSource = TracksGroupedByAlbum.View.CollectionGroups;
        }

        private void ArtistAlbumsList_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= CurrentOnSizeChanged;
        }

        private void CurrentOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
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
    }
}
