using VLC.Helpers;
using VLC.Model;
using VLC.ViewModels;
using VLC.Universal8._1.Views.MainPages;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using VLC.Universal8._1.Views.UserControls;
using VLC.Universal8._1.Views.UserControls.Shell;

namespace VLC.Universal8._1.Views.MainPages
{
    public sealed partial class HomePage : Page
    {
        public HomePage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            MyGrid.Children.Clear();
            if (Locator.SettingsVM.MediaCenterMode)
            {
                MyGrid.Children.Add(new HomePageController());
            }
            else
            {
                MyGrid.Children.Add(new DesktopHomePageController());
                await AppViewHelper.SetAppView(true);
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            if (!Locator.SettingsVM.MediaCenterMode)
                await AppViewHelper.SetAppView(false);
        }
    }
}
