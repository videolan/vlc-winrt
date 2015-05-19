using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using VLC_WinRT.Model.Music;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class TrackItemTemplateDetailed : UserControl
    {
        public TrackItemTemplateDetailed()
        {
            this.InitializeComponent();
        }
        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((Grid)sender);
        }

        private void NameTextBlock_OnLoaded(object sender, RoutedEventArgs e)
        {
            var trackItem = this.DataContext as TrackItem;
            var b = new Binding
            {
                Converter = App.Current.Resources["CurrentTrackEnhancifierConverter"] as IValueConverter,
                ConverterParameter = ((TextBlock)sender).Foreground,
                ConverterLanguage = "color",
                Source = trackItem,
                Path = new PropertyPath("IsCurrentPlaying"),
            };
            ((TextBlock)sender).SetBinding(TextBlock.ForegroundProperty, b);
        }
    }
}
