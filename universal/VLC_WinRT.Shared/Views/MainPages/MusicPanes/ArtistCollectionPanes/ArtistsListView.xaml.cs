using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Music;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.MainPages.MusicPanes.ArtistCollectionPanes
{
    public sealed partial class ArtistsListView : UserControl
    {
        private bool isNarrow = false;
        public ArtistsListView()
        {
            this.InitializeComponent();
            this.Loaded += ArtistCollectionBase_Loaded;
        }

        void ArtistCollectionBase_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += ArtistCollectionBase_Unloaded;
            Responsive();
        }

        void ArtistCollectionBase_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        async void Responsive()
        {
        }

        private void ArtistListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as ArtistItem;
            if (Window.Current.Bounds.Width >= 800)
                Locator.MusicLibraryVM.CurrentArtist = artist;
            else Locator.MusicLibraryVM.ArtistClickedCommand.Execute(artist);
        }
    }
}
