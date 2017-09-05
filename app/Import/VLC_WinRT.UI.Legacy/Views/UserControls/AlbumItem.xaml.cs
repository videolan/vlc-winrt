using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using VLC.Utils;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class AlbumItem : UserControl
    {
        public AlbumItem()
        {
            this.InitializeComponent();
        }
        
        private void RootAlbumItem_Holding(object sender, HoldingRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Grid)sender);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Grid)sender);
        }

        private void Cover_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse) return;
            VisualStateManager.GoToState(this, "MouseOver", false);
        }

        private void Cover_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType != Windows.Devices.Input.PointerDeviceType.Mouse) return;
            VisualStateManager.GoToState(this, "Normal", false);
        }
        

        public AlbumItem Album
        {
            get { return (AlbumItem)GetValue(AlbumProperty); }
            set { SetValue(AlbumProperty, value); }
        }

        public static readonly DependencyProperty AlbumProperty =
            DependencyProperty.Register(nameof(Album), typeof(AlbumItem), typeof(AlbumItem), new PropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (AlbumItem)dependencyObject;
            that.Init();
        }

        public void Init()
        {
            if (Album == null) return;
            NameTextBlock.Text = Strings.HumanizedAlbumName(Album.Name);
            //ArtistTextBlock.Text = Album.ArtistTextBlock.Text;
            
            
            //ButtonOverlay.Command = .PlayAlbum;
            ButtonOverlay.CommandParameter = Album;

            //Album.PropertyChanged += Album_PropertyChanged;
            var album = Album;
            //Task.Run(async () =>
            //{
            //    await album.ResetAlbumArt();
            //});
        }

        private async void Album_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //if (e.PropertyName == nameof(Album.AlbumImage))
            //{
            //    if (Album == null)
            //        return;
            //    if (Album?.AlbumImage?.UriSource == (Cover.Source as BitmapImage)?.UriSource)
            //        return;
            //    await DispatchHelper.InvokeAsync(CoreDispatcherPriority.Low, () =>
            //    {
            //        FadeOutCover.Begin();
            //    });
            //}
        }

        private void FadeOutCover_Completed(object sender, object e)
        {
            //if (Album != null && Album.AlbumImage != null)
            //{
            //    Cover.Source = Album.AlbumImage;
            //    FadeInCover.Begin();
            //}
        }
    }
}
