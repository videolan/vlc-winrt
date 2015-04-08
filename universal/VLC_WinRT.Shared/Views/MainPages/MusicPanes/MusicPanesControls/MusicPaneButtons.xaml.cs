using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WinRT.Views.MainPages.MusicPanes.MusicPanesControls
{
    public sealed partial class MusicPaneButtons : UserControl
    {
        public MusicPaneButtons()
        {
            this.InitializeComponent();
            this.Loaded += VideoPaneButtons_Loaded;
        }

        void VideoPaneButtons_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(Window.Current.Bounds.Width);
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += MusicPaneButtons_Unloaded;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive(e.Size.Width);
        }

        void MusicPaneButtons_Unloaded(object sender, RoutedEventArgs e)
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
            if (string.IsNullOrEmpty(MusicSearchBox.Text) && !string.IsNullOrEmpty(Locator.MusicLibraryVM.SearchTag))
                Locator.MainVM.ChangeMainPageMusicViewCommand.Execute((int)Locator.SettingsVM.MusicView);
            Locator.MusicLibraryVM.SearchTag = MusicSearchBox.Text;
        }
    }
}
