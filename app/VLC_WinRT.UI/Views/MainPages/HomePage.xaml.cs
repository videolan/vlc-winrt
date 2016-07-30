using Microsoft.Xaml.Interactivity;
using VLC.Helpers;
using VLC.Model;
using VLC.ViewModels;
using VLC.Views.MainPages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace VLC.UI.Legacy.Views.MainPages
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
            AppViewHelper.SetAppView(true);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            AppViewHelper.SetAppView(false);
        }
    }
}
