using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC.Model.Music;
using VLC.Utils;
using VLC.ViewModels;

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
            //AvatarEllipse.Height = NoAvatarEllipse.Height = AvatarColumnDefinition.ActualWidth;
        }

        public ArtistItem Artist
        {
            // Changed to being Soft cast because this is not always ArtistItem, causing music view to break
            // TODO: Figure out why this is not always ArtistItem 
            get { return GetValue(ArtistProperty) as ArtistItem; }
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
            // Related to change above, ArtistItem is not being set correctly
            // Causing artist to be null.
            // This "fixes" the issue, but needs a deep look into why it's happening.
            if (Artist == null)
                return;

            //NameTextBlock.Text = Strings.HumanizedArtistName(Artist.Name);
            Artist.PropertyChanged += Artist_PropertyChanged;
            
            var artist = Artist;
            Task.Run(async () =>
            {
                artist.ResetArtistPicture(true);
                var albumsCount = Locator.MediaLibrary.LoadAlbumsCount(artist.Id);
                //await DispatchHelper.InvokeInUIThreadAsync(CoreDispatcherPriority.Low, () => AlbumsCountTextBlock.Text = Strings.Albums.ToUpperFirstChar() + Strings.Dash + albumsCount);
            });
        }

        private async void Artist_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Artist.ArtistImageThumbnail))
            {
                if (Artist == null) return;
                await DispatchHelper.InvokeInUIThreadAsync(CoreDispatcherPriority.Low, () =>
                {
                    FadeOutCover.Begin();
                });
            }
        }

        private void FadeOutCover_Completed(object sender, object e)
        {
            if (Artist != null && Artist.ArtistImageThumbnail != null)
            {
                //ArtistImageBrush.ImageSource = Artist.ArtistImageThumbnail;
                FadeInCover.Begin();
            }
        }
    }
}
