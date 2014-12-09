using System;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Views.MainPages
{
    public sealed partial class MainPageSDCard : UserControl
    {
        public MainPageSDCard()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Unloaded += OnUnloaded;
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (Locator.ExternalStorageVM.CurrentStorageVM.CanGoBack)
            {
                backPressedEventArgs.Handled = true;
                Locator.ExternalStorageVM.CurrentStorageVM.GoBackCommand.Execute(null);
            }
            else
            {
                App.ApplicationFrame.Navigate(typeof(MainPageHome));
            }
        }

    }
}
