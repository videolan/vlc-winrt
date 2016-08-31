using Microsoft.Xaml.Interactivity;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace VLC.UI.Legacy.Views.MusicPages.ArtistPageControls
{
    public sealed partial class MainArtistHeader : UserControl
    {
        public MainArtistHeader()
        {
            this.InitializeComponent();
            this.Loaded += MainArtistHeader_Loaded;
        }
        
        private async void MainArtistHeader_Loaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded += MainArtistHeader_Unloaded;
            Window.Current.SizeChanged += Current_SizeChanged;
            Responsive();

            Locator.MusicLibraryVM.CurrentArtist.PropertyChanged += CurrentArtist_PropertyChanged;
            await Locator.MusicLibraryVM.CurrentArtist.ResetArtistPicture(true);
            await Locator.MusicLibraryVM.CurrentArtist.ResetArtistPicture(false);
        }

        private async void CurrentArtist_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Locator.MusicLibraryVM.CurrentArtist == null) return;
            if (e.PropertyName == nameof(ArtistItem.ArtistImageThumbnail))
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    EllipseImage.Fill = new ImageBrush()
                    {
                        ImageSource = Locator.MusicLibraryVM.CurrentArtist.ArtistImageThumbnail,
                        Stretch = Stretch.UniformToFill
                    };
                });
            }
            else if (e.PropertyName == nameof(ArtistItem.ArtistImage))
            {
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => BackgroundImage.Source = Locator.MusicLibraryVM.CurrentArtist.ArtistImage);
            }
        }

        private void MainArtistHeader_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            BackgroundEffect.StartAnimation();
            if (Window.Current.Bounds.Width < 650)
            {
                VisualStateUtilities.GoToState(this, "Snap", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Wide", false);
            }

            if (Window.Current.Bounds.Height < 600)
            {
                //HeaderGrid.Height = 100;
                VisualStateUtilities.GoToState(this, "Tiny", false);
            }
            else
            {
                VisualStateUtilities.GoToState(this, "Tall", false);
            }
        }
    }
}
