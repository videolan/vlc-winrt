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

        public bool IsFlyoutEnabled
        {
            get { return (bool)GetValue(IsFlyoutEnabledProperty); }
            set { SetValue(IsFlyoutEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFlyoutEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlyoutEnabledProperty =
            DependencyProperty.Register("IsFlyoutEnabled", typeof(bool), typeof(TrackItemTemplateDetailed), new PropertyMetadata(true));

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (IsFlyoutEnabled)
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

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (IsFlyoutEnabled)
                Flyout.ShowAttachedFlyout((Grid)sender);
        }
    }
}
