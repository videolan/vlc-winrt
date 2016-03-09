using Microsoft.Xaml.Interactivity;
using VLC_WinRT.Model;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VLC_WinRT.UI.Legacy.Views.MainPages
{
    public sealed partial class HomePage : Page
    {
        public VLCPage CurrentHomePage
        {
            get; private set;
        }
        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
    }
}
