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