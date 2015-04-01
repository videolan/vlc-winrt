using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Views.VariousPages
{
    public sealed partial class SpecialThanks : Page
    {
        public SpecialThanks()
        {
            this.InitializeComponent();
        }
        
        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as ItemsWrapGrid).ItemWidth = Window.Current.Bounds.Width/2;
        }
    }
}
