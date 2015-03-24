using VLC_WINRT_APP.Views.MusicPages.AlbumPageControls;
using VLC_WINRT_APP.Helpers;
using System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif
namespace VLC_WINRT_APP.Views.MusicPages
{
    public sealed partial class AlbumPage : Page
    {
        public AlbumPage()
        {
            this.InitializeComponent();
        }

#if WINDOWS_PHONE_APP
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Loaded += AlbumPage_Loaded;
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        void AlbumPage_Loaded(object sender, RoutedEventArgs e)
        {
            this.Content = new AlbumPageBase();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.Loaded -= AlbumPage_Loaded;
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            App.ApplicationFrame.GoBack();
            backPressedEventArgs.Handled = true;
        }
#endif
    }
}
