using System;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageSDCard : Page
    {
        public MainPageSDCard()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
                backPressedEventArgs.Handled = true;
            if (Locator.ExternalStorageVM.CurrentStorageVM.CanGoBack)
            {
                Locator.ExternalStorageVM.CurrentStorageVM.GoBackCommand.Execute(null);
            }
            else
            {
                App.ApplicationFrame.Navigate(typeof (MainPageHome));
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }
    }
}
