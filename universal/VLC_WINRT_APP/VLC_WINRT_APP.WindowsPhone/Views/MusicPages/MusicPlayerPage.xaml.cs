using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Helpers;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class MusicPlayerPage : Page
    {
        public MusicPlayerPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await AppBarHelper.UpdateAppBar(typeof (MusicPlayerPage));
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            App.ApplicationFrame.GoBack();
            backPressedEventArgs.Handled = true;
        }

        private void VariableSizedWrapGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();
            ((VariableSizedWrapGrid)sender).ItemHeight = (displayInformation.LogicalDpi < 200) ? 150 : 200;
            ((VariableSizedWrapGrid)sender).ItemWidth = (displayInformation.LogicalDpi < 200) ? 150 : 200;
        }
    }
}
