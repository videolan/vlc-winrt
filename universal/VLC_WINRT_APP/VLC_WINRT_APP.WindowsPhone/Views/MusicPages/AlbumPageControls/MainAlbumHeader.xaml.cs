using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC_WINRT_APP.Views.MusicPages.AlbumPageControls
{
    public sealed partial class MainAlbumHeader : UserControl
    {
        private bool isminimized = false;
        private double originalHeight;
        public MainAlbumHeader()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged += OnSizeChanged;
            originalHeight = this.ActualHeight;
            this.Unloaded += OnUnloaded;
        }

        private async void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (this.ActualHeight < 170 && !isminimized)
            {
                isminimized = true;
                AlbumsCommandRowDefinition.Height = new GridLength(0, GridUnitType.Pixel);
                AlbumCommandsPanel.FadeOut();
            }
            else if (this.ActualHeight > 170 && isminimized)
            {
                isminimized = false;
                AlbumsCommandRowDefinition.Height = new GridLength(1, GridUnitType.Auto);
            }
            else if (this.ActualHeight == originalHeight)
            {
                AlbumCommandsPanel.FadeIn();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }

        private void SwypeRightToLeft_Button_Click(object sender, RoutedEventArgs e)
        {
            var albumPage = App.ApplicationFrame.Content as AlbumPage;
            if (albumPage != null)
                Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => albumPage.HeaderFlipView.SelectedIndex = 1);
        }
    }
}
