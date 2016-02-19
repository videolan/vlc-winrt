using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class ArtistHorizontalTemplate : UserControl
    {
        public ArtistHorizontalTemplate()
        {
            this.InitializeComponent();
            this.SizeChanged += ArtistHorizontalTemplate_SizeChanged;
        }

        private void ArtistHorizontalTemplate_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AvatarEllipse.Height = NoAvatarEllipse.Height = AvatarColumnDefinition.ActualWidth;
        }

        public ArtistItem Artist
        {
            get { return (ArtistItem)GetValue(ArtistProperty); }
            set { SetValue(ArtistProperty, value); }
        }

        public static readonly DependencyProperty ArtistProperty =
            DependencyProperty.Register(nameof(Artist), typeof(ArtistItem), typeof(ArtistHorizontalTemplate), new PropertyMetadata(0, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (ArtistHorizontalTemplate)dependencyObject;
            that.Init();
        }
        
        public void Init()
        {
            NameTextBlock.Text = Strings.HumanizedArtistName(Artist.Name);
            Artist.PropertyChanged += Artist_PropertyChanged;
            
            var artist = Artist;
            Task.Run(async () =>
            {
                await artist.ResetArtistHeader();
                var albumsCount = await Locator.MusicLibraryVM.MusicLibrary.LoadAlbumsCount(artist.Id);
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () => AlbumsCountTextBlock.Text = albumsCount + " " + Strings.Albums);
            });
        }

        private async void Artist_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Artist.ArtistImage))
            {
                if (Artist == null) return;
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    FadeOutCover.Begin();
                });
            }
        }

        private void FadeOutCover_Completed(object sender, object e)
        {
            if (Artist != null && Artist.ArtistImage != null)
            {
                ArtistImageBrush.ImageSource = Artist.ArtistImage;
                FadeInCover.Begin();
            }
        }
    }
}
