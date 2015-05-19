using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Views.MusicPages.MusicNowPlayingControls
{
    public sealed partial class SecondaryMusicPlayerButtons : Grid
    {
        public SecondaryMusicPlayerButtons()
        {
            this.InitializeComponent();
        }
        
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            "FontSize", typeof (double), typeof (SecondaryMusicPlayerButtons), new PropertyMetadata(30, FontSizePropertyChangedCallback));

        private static void FontSizePropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (SecondaryMusicPlayerButtons)dependencyObject;
            var size = (double)dependencyPropertyChangedEventArgs.NewValue;
            that.ShuffleFontIcon.FontSize = size;
            that.ShareFontIcon.FontSize = size;
            that.FavoriteFontIcon.FontSize = size;
            that.RepeatFontIcon.FontSize = size;
            that.PlaylistPath.Height = size - 2;
            that.PlaylistPath.Width = size - 2;
        }

        public double FontSize
        {
            get { return (double) GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
    }
}
