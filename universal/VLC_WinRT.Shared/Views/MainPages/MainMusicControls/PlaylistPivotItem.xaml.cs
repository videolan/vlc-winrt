using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;

namespace VLC_WinRT.Views.MainPages.MainMusicControls
{
    public sealed partial class PlaylistPivotItem : Page
    {
        public PlaylistPivotItem()
        {
            this.InitializeComponent();
        }

        private void Collection_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
            Window.Current.SizeChanged += CurrentOnSizeChanged;
            this.Unloaded += OnUnloaded;
        }

        private void CurrentOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            Responsive();
        }

        void Responsive()
        {
            if (Window.Current.Bounds.Width > 700)
            {
                VisualStateUtilities.GoToState(this, "Horizontal", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Vertical", false);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Window.Current.SizeChanged -= CurrentOnSizeChanged;
        }
    }
}
