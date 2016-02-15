using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Utils;
using Windows.UI.Xaml.Data;
using ScrollWatcher;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class ArtistCollectionBase : UserControl
    {
        public ArtistCollectionBase()
        {
            this.InitializeComponent();
            this.Loaded += ArtistCollectionBase_Loaded;
            this.Unloaded += ArtistCollectionBase_Unloaded;
        }

        private void ArtistCollectionBase_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Locator.NavigationService.CurrentPage != Model.VLCPage.ArtistPage)
            {
                Locator.MusicLibraryVM.OnNavigatedFromArtists();
            }
        }

        void ArtistCollectionBase_Loaded(object sender, RoutedEventArgs e)
        {
            Locator.MusicLibraryVM.OnNavigatedToArtists();
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            ArtistsZoomedOutView.ItemsSource = GroupArtists.View.CollectionGroups;
        }
    }
}