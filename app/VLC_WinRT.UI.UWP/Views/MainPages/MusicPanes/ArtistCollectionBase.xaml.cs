using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Model.Video;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class ArtistCollectionBase : Page
    {
        public ArtistCollectionBase()
        {
            this.InitializeComponent();
            this.Loaded += ArtistCollectionBase_Loaded;
            this.Unloaded += ArtistCollectionBase_Unloaded;
        }

        private void ArtistCollectionBase_Unloaded(object sender, RoutedEventArgs e)
        {
            Locator.MusicLibraryVM.OnNavigatedFromArtists();
            App.SetShellDecoration(false);
        }

        void ArtistCollectionBase_Loaded(object sender, RoutedEventArgs e)
        {
            Locator.MusicLibraryVM.OnNavigatedToArtists();
        }

        private void SemanticZoom_OnViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            ArtistsZoomedOutView.ItemsSource = GroupArtists.View.CollectionGroups;
        }

        private void ArtistListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.SetShellDecoration(true, true);
        }
    }
}