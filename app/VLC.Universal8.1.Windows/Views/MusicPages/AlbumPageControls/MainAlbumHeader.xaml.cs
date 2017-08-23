using System.Diagnostics;
using VLC.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC.Universal8._1.Views.MusicPages.AlbumPageControls
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
            PlayAlbum.Focus(FocusState.Keyboard);
        }
        
        private void ViewArtistButton_Click(object sender, RoutedEventArgs e)
        {
            Locator.NavigationService.GoBack_HideFlyout();
        }
    }
}
