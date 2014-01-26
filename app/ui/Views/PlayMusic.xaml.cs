using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC_WINRT.Utility.Services.RunTime;

namespace VLC_WINRT.Views
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class PlayMusic : Page
    {
        public PlayMusic()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FadeInPage.Begin();
        }

        private async void GoBack_Click(object sender, RoutedEventArgs e)
        {
            await FadeOutPage.BeginAsync();
            NavigationService.NavigateTo(typeof(MainPage));
        }
    }
}
