using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class MusicPlayerPage : Page
    {
        public MusicPlayerPage()
        {
            this.InitializeComponent();
        }

        private async void PlayPauseHold(object sender, HoldingRoutedEventArgs e)
        {
            Locator.MusicPlayerVM.PlayOrPauseCommand.Execute(null);
            await Locator.MusicPlayerVM.CleanViewModel();
            if(App.ApplicationFrame.CanGoBack)
                App.ApplicationFrame.GoBack();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
            Locator.MainVM.CommandBar.Background = new SolidColorBrush(Color.FromArgb(200, 50,50,50));
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
            Locator.MainVM.CommandBar.Background = (SolidColorBrush)App.Current.Resources["MainColor"];
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
            //Debug.WriteLine(displayInformation.LogicalDpi);
        }
    }
}
