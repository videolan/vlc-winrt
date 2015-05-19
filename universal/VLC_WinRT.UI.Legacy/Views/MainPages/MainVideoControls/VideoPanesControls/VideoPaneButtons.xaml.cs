using Microsoft.Xaml.Interactivity;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC_WinRT.Views.MainPages.MainVideoControls.VideoPanesControls
{
    public sealed partial class VideoPaneButtons : UserControl
    {
        public VideoPaneButtons()
        {
            this.InitializeComponent();
            VideoSearchBox.AddHandler(TappedEvent, new TappedEventHandler(VideoSearchBox_Tapped), true);
            this.Loaded += VideoPaneButtons_Loaded;
        }

        void VideoPaneButtons_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(Window.Current.Bounds.Width);
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += VideoPaneButtons_Unloaded;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive(e.Size.Width);
        }

        void VideoPaneButtons_Unloaded(object sender, RoutedEventArgs e)
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
            if (string.IsNullOrEmpty(VideoSearchBox.Text) && !string.IsNullOrEmpty(Locator.VideoLibraryVM.SearchTag))
                Locator.MainVM.ChangeMainPageVideoViewCommand.Execute((int)Locator.SettingsVM.VideoView);
            Locator.VideoLibraryVM.SearchTag = VideoSearchBox.Text;
        }

        private void VideoSearchBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Locator.MainVM.ChangeMainPageVideoViewCommand.Execute(3);
        }
    }
}
