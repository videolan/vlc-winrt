using VLC_WinRT.Views.MusicPages.ArtistPageControls;
using VLC_WinRT.Helpers;
using Windows.UI.Xaml.Controls;
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
using Windows.UI.Xaml.Navigation;
#endif
namespace VLC_WinRT.Views.MusicPages
{
    public sealed partial class ArtistPage : Page
    {
        public ArtistPage()
        {
            this.InitializeComponent();
        }
#if WINDOWS_PHONE_APP
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.Loaded -= ArtistPage_Loaded;
            HardwareButtons.BackPressed -= HardwareButtonsOnBackPressed;
        }

        private void HardwareButtonsOnBackPressed(object sender, BackPressedEventArgs backPressedEventArgs)
        {
            App.ApplicationFrame.GoBack();
            backPressedEventArgs.Handled = true;
        }

        /// <summary>
        /// Invoqué lorsque cette page est sur le point d'être affichée dans un frame.
        /// </summary>
        /// <param name="e">Données d'événement décrivant la manière dont l'utilisateur a accédé à cette page.
        /// Ce paramètre est généralement utilisé pour configurer la page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.Loaded += ArtistPage_Loaded;
            HardwareButtons.BackPressed += HardwareButtonsOnBackPressed;
        }

        void ArtistPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Content = new ArtistPageBase();
        }
#endif

    }
}
