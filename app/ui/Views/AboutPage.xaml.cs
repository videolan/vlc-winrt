using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace VLC_WINRT.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AboutPage : Page
    {
        public AboutPage()
        {
            this.InitializeComponent();
        }

        private void AboutAppVersionTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            versionNum.Text = Package.Current.Id.Version.Major.ToString() + "." + Package.Current.Id.Version.Minor.ToString();
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}
