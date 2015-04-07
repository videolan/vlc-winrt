using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Views.MainPages.MusicPanes
{
    public sealed partial class SearchMusicPane : UserControl
    {
        public SearchMusicPane()
        {
            this.InitializeComponent();
            this.Loaded += SearchMusicPane_Loaded;
        }

        void SearchMusicPane_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(Window.Current.Bounds.Width);
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += SearchMusicPane_Unloaded;
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
            if (width <= 550)
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
