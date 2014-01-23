using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateTo(typeof(MainPage));
        }
    }
}
