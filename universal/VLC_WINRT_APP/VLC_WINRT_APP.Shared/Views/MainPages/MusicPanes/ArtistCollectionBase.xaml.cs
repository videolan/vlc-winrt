using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using VLC_WINRT_APP.Helpers;
using VLC_WINRT_APP.Model.Video;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.Views.MainPages.MusicPanes
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
            if (Window.Current.Bounds.Width < 800)
            {
                VisualStateUtilities.GoToState(this, "Narrow", false);
            }
            else if (Window.Current.Bounds.Width < 900)
            {
                VisualStateUtilities.GoToState(this, "Medium", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
            }
        }
    }
}
