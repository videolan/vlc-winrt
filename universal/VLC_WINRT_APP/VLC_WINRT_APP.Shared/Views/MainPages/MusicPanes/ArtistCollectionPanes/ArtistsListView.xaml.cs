using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.Model.Video;
using VLC_WINRT_APP.ViewModels;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.Views.MainPages.MusicPanes.ArtistCollectionPanes
{
    public sealed partial class ArtistsListView : UserControl
    {
        public ArtistsListView()
        {
            this.InitializeComponent();
            this.Loaded += ArtistCollectionBase_Loaded;
        }

        void ArtistCollectionBase_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += ArtistCollectionBase_Unloaded;
        }

        void ArtistCollectionBase_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            var wrapGrid = (ArtistListView).GetFirstDescendantOfType<ItemsWrapGrid>();
            if (Window.Current.Bounds.Width < 900)
            {
                wrapGrid.Orientation = Orientation.Horizontal;
                TemplateSizer.ComputeAlbums(wrapGrid, TemplateSize.Normal, this.ActualWidth);
                VisualStateUtilities.GoToState(this, "Narrow", false);
            }
            else
            {
                wrapGrid.Orientation = Orientation.Vertical;
                wrapGrid.ItemHeight = 60;
                wrapGrid.ItemWidth = this.ActualWidth;
                VisualStateUtilities.GoToState(this, "Wide", false);
            }
        }

        private void ArtistListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as ArtistItem;
            if (Window.Current.Bounds.Width > 900)
                Locator.MusicLibraryVM.CurrentArtist = artist;
            else Locator.MusicLibraryVM.ArtistClickedCommand.Execute(artist);
        }
    }
}
