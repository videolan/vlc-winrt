using VLC.Model.Video;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using VLC.ViewModels;
using VLC.Model.Music;
using VLC.Utils;
using Windows.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class AlbumWithTracksResponsiveTemplate : UserControl
    {
        public AlbumWithTracksResponsiveTemplate()
        {
            this.InitializeComponent();
            this.Loaded += AlbumWithTracksResponsiveTemplate_Loaded;
        }

        private void AlbumWithTracksResponsiveTemplate_Loaded(object sender, RoutedEventArgs e)
        {
            this.SizeChanged += AlbumWithTracksResponsiveTemplate_SizeChanged;
            this.KeyDown += AlbumWithTracksResponsiveTemplate_KeyDown;
            Responsive();
        }

        private void AlbumWithTracksResponsiveTemplate_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.OriginalKey == Windows.System.VirtualKey.GamepadLeftThumbstickRight)
            {
                (FocusManager.FindNextFocusableElement(FocusNavigationDirection.Right) as Control)?.Focus(FocusState.Keyboard);
            }
            else if (e.OriginalKey == Windows.System.VirtualKey.GamepadLeftThumbstickDown)
            {
                (FocusManager.FindNextFocusableElement(FocusNavigationDirection.Down) as Control)?.Focus(FocusState.Keyboard);
            }
        }

        private void AlbumWithTracksResponsiveTemplate_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Responsive();
        }

        void Responsive()
        {
            if (this.ActualWidth > 900)
            {
                CoverImage.Width = CoverImage.Height = HeaderGrid.Height = 120;
            }
            else
            {
                CoverImage.Width = CoverImage.Height = HeaderGrid.Height = 70;
            }
        }

        public AlbumItem Album
        {
            get { return (AlbumItem)GetValue(AlbumProperty); }
            set { SetValue(AlbumProperty, value); }
        }

        public static readonly DependencyProperty AlbumProperty =
            DependencyProperty.Register(nameof(Album), typeof(AlbumItem), typeof(AlbumWithTracksResponsiveTemplate), new PropertyMetadata(null, PropertyChangedCallback));


        private static async void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (AlbumWithTracksResponsiveTemplate)dependencyObject;
            await that.Init();
        }

        public async Task Init()
        {
            if (Album == null) return;
            NameTextBlock.Text = Utils.Strings.HumanizedAlbumName(Album.Name);

            PlayAppBarButton.Label = Utils.Strings.PlayAlbum;
            PlayAppBarButton.Command = Locator.MusicLibraryVM.PlayAlbumCommand;
            PlayAppBarButton.CommandParameter = Album;

            PinAppBarButton.Command = Album.PinAlbumCommand;
            PinAppBarButton.CommandParameter = Album;

            FavoriteAppBarButton.Command = Album.FavoriteAlbum;
            FavoriteAppBarButton.CommandParameter = Album;

            Album.PropertyChanged += Album_PropertyChanged;
            var album = Album;
            await album.ResetAlbumArt();
        }

        private async void Album_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Album.AlbumImage))
            {
                if (Album == null) return;
                if (Album?.AlbumImage?.UriSource == (CoverImage.Source as BitmapImage)?.UriSource) return;
                await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
                {
                    FadeOutCover.Begin();
                });
            }
        }
        
        private void FadeOutCover_Completed(object sender, object e)
        {
            if (Album != null && Album.AlbumImage != null)
            {
                CoverImage.Source = Album.AlbumImage;
                FadeInCover.Begin();
            }
        }
    }
}