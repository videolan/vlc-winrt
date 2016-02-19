using VLC_WinRT.Model.Video;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model.Music;
using VLC_WinRT.Utils;
using Windows.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;

#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif

namespace VLC_WinRT.UI.Legacy.Views.UserControls
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
            Responsive();
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
            NameTextBlock.Text = Strings.HumanizedAlbumName(Album.Name);

            PlayAppBarButton.Label = Strings.PlayAlbum;
            PlayAppBarButton.Command = Locator.MusicLibraryVM.PlayAlbumCommand;
            PlayAppBarButton.CommandParameter = Album;

            PinAppBarButton.Command = Album.PinAlbumCommand;
            PinAppBarButton.CommandParameter = Album;

            FavoriteAppBarButton.Command = Album.FavoriteAlbum;
            FavoriteAppBarButton.CommandParameter = Album;
        }
    }
}