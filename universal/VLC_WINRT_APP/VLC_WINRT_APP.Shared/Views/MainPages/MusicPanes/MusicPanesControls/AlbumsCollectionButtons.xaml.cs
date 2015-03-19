
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WINRT_APP.Views.MainPages.MusicPanes.MusicPanesControls
{
    public sealed partial class AlbumsCollectionButtons : UserControl
    {
        public AlbumsCollectionButtons()
        {
            this.InitializeComponent();
            this.Loaded += AlbumsCollectionButtons_Loaded;
        }

        void AlbumsCollectionButtons_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Responsive(Window.Current.Bounds.Width);
            Window.Current.SizeChanged += Current_SizeChanged;
            this.Unloaded += AlbumsCollectionButtons_Unloaded;
        }

        void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive(e.Size.Width);
        }

        void AlbumsCollectionButtons_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        void Responsive(double width)
        {
#if WINDOWS_PHONE_APP
            if (width < 470)
#else
            if (width < 570)
#endif
                VisualStateUtilities.GoToState(this, "Minimal", false);
            else
                VisualStateUtilities.GoToState(this, "Normal", false);
        }
    }
}
