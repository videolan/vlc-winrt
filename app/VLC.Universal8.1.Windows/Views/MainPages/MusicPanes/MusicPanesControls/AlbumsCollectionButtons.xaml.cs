using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.ViewModels;

namespace VLC.Universal8._1.Views.MainPages.MusicPanes.MusicPanesControls
{
    public sealed partial class AlbumsCollectionButtons : UserControl
    {
        public AlbumsCollectionButtons()
        {
            this.InitializeComponent();
            this.Loaded += AlbumsCollectionButtons_Loaded;
        }

        void AlbumsCollectionButtons_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive(Window.Current.Bounds.Width);
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += AlbumsCollectionButtons_Unloaded;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive(e.Size.Width);
        }

        void AlbumsCollectionButtons_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        void Responsive(double width)
        {
          //  if (width < 700)
           //     VisualStateUtilities.GoToState(this, "Minimal", false);
            //else
             //   VisualStateUtilities.GoToState(this, "Normal", false);
        }

        private void ComboBox_OnDropDownOpened(object sender, object e)
        {
            Locator.NavigationService.PreventAppExit = true;
        }

        private void ComboBox_OnDropDownClosed(object sender, object e)
        {
            Locator.NavigationService.PreventAppExit = false;
        }
    }
}
