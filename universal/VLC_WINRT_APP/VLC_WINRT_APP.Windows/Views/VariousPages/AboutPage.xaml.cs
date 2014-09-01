using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238
using Windows.UI.Xaml.Navigation;

namespace VLC_WINRT_APP.Views.VariousPages
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            List<DevItem> Devs = new List<DevItem>();
            Devs.Add(new DevItem("Kellen Sunderland", "developer", "KellenDB"));
            Devs.Add(new DevItem("Thomas Nigro", "designer, developer", "ThomasNigro"));
            Devs.Add(new DevItem("Jean Baptiste Kempf", "developer", "videolan"));
            Devs.Add(new DevItem("Hugo Beauzée-Luyssen", "developer", "beauzeh"));
            Devs.Add(new DevItem("Timothy Miller", "developer", "drasticactionSA"));
            Devs.Add(new DevItem("Sébastien Thevenin", "developer", "SebThevenin"));
            DevsListView.ItemsSource = Devs;
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}

public class DevItem
{
    public string Name { get; set; }
    public string Role { get; set; }
    public string TwitterURI { get; set; }
    public string LinkedinURI { get; set; }

    public DevItem(string name, string role, string twitter = null, string linkedin = null)
    {
        Name = name;
        Role = role;
        TwitterURI = "https://twitter.com/" + twitter;
        LinkedinURI = linkedin;
    }
}
