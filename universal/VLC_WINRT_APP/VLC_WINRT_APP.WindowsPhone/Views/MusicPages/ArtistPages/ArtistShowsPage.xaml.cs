using System;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT_APP.Model.Music;

namespace VLC_WINRT_APP.Views.MusicPages.ArtistPages
{
    public sealed partial class ArtistShowsPage : Page
    {
        public ArtistShowsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            if (App.ApplicationFrame.CanGoBack)
            {
                App.ApplicationFrame.GoBack();
            }
            backPressedEventArgs.Handled = true;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }
    }
}
