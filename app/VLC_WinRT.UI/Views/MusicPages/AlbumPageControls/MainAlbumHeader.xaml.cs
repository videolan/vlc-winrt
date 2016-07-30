using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC.Views.MusicPages.AlbumPageControls
{
    public sealed partial class MainAlbumHeader : UserControl
    {
        public MainAlbumHeader()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged += OnSizeChanged;
            this.Unloaded += OnUnloaded;
            PlayAlbum.Focus(FocusState.Keyboard);
        }
        
        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }
    }
}
