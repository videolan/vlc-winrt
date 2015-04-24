using System.Linq;
using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Video;
using VLC_WinRT.Views.MainPages.MusicPanes.MusicPanesControls;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class SearchMusicPane : UserControl
    {
        public SearchMusicPane()
        {
            this.InitializeComponent();
            this.Loaded += SearchMusicPane_Loaded;
        }

        private void MusicWrapGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TemplateSizer.ComputeAlbums(sender as ItemsWrapGrid, this.ActualWidth);
        }

        void SearchMusicPane_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(Window.Current.Bounds.Width);
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += SearchMusicPane_Unloaded;

            var artistPane = (App.ApplicationFrame.Content as MainPageMusic).GetDescendantsOfType<ArtistPaneButtons>().FirstOrDefault(x => x.Visibility == Visibility.Visible);
            var textbox = artistPane.GetFirstDescendantOfType<TextBox>();
            textbox.Focus(FocusState.Keyboard);
            textbox.SelectionStart = textbox.Text.Length;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive(e.Size.Width);
        }

        void SearchMusicPane_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        void Responsive(double width)
        {
            if (width <= 650)
                VisualStateUtilities.GoToState(this, "Minimal", false);
            else
                VisualStateUtilities.GoToState(this, "Normal", false);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Locator.MusicLibraryVM.SearchTag = MusicSearchBox.Text;
        }
    }
}
