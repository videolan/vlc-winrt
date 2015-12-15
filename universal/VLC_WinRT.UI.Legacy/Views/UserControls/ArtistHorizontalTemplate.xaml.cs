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
            var id = Artist.Id;
            Task.Run(async () =>
            {
                var albumsCount = await Locator.MusicLibraryVM.MusicLibrary.LoadAlbumsCount(id);
                await App.Dispatcher.RunAsync(CoreDispatcherPriority.Low,
                        () => AlbumsCountTextBlock.Text = albumsCount + " " + Strings.Albums);
            });
        }
    }
}
