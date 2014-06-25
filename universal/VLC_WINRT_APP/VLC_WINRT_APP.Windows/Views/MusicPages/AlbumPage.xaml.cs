using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace VLC_WINRT_APP.Views.MusicPages
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class AlbumPage : Page
    {
        public AlbumPage()
        {
            this.InitializeComponent();
            this.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            if (Window.Current.Bounds.Width < 400)
            {
                FirstRowDefinition.Height = new GridLength(0);
                CoverImage.Width = 130;
                CoverImage.Height = 130;
                CoverImageColumnDefinition.Width = new GridLength(130);
                CoverArtistRowDefinition.Height = new GridLength(200);
            }
            else
            {
                FirstRowDefinition.Height = new GridLength(42);
                CoverImage.Width = 200;
                CoverImage.Height = 200;
                CoverImageColumnDefinition.Width = new GridLength(200);
                CoverArtistRowDefinition.Height = new GridLength(350);
            }
        }

        #region interactions
        private void GoBack(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
        #endregion
    }
}
