using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Views.VariousPages
{
    public sealed partial class SpecialThanks : UserControl
    {
        bool isSponsorsView = true;
        public SpecialThanks()
        {
            this.InitializeComponent();
        }

        private void ItemsWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as ItemsWrapGrid).ItemWidth = Window.Current.Bounds.Width / 2;
        }

        private void SwitchList_ClickPeople(object sender, RoutedEventArgs e)
        {
            isSponsorsView = false;
            Switch();
        }
        private void SwitchList_ClickSponsors(object sender, RoutedEventArgs e)
        {
            isSponsorsView = true;
            Switch();
        }

        void Switch()
        {
            if (isSponsorsView)
            {
                People.Visibility = Visibility.Collapsed;
                Sponsors.Visibility = Visibility.Visible;
                SponsorsButton.Opacity = 1;
                AllButton.Opacity = 0.6;
            }
            else
            {
                People.Visibility = Visibility.Visible;
                Sponsors.Visibility = Visibility.Collapsed; SponsorsButton.Opacity = 0.6;
                AllButton.Opacity = 1;
            }
        }
    }
}
