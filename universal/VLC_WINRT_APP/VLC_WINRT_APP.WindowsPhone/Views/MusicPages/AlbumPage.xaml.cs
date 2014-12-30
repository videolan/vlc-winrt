using System;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class AlbumPage : Page
    {
        public AlbumPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
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
    }
}
