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
            Locator.MainVM.PropertyChanged += MainVM_PropertyChanged;
            Navigate(Locator.MainVM.CurrentPanel.Target);
        }

        private void MainVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainVM.CurrentPanel))
            {
                Navigate(Locator.MainVM.CurrentPanel.Target);
            }
        }

        public void Navigate(VLCPage page)
        {
            CurrentHomePage = page;
            switch (page)
            {
                case VLCPage.MainPageVideo:
                    HomePageController.HomePageContentPresenter.Content = new MainPageVideos();
                    break;
                case VLCPage.MainPageMusic:
                    HomePageController.HomePageContentPresenter.Content = new MainPageMusic();
                    break;
                case VLCPage.MainPageFileExplorer:
                    HomePageController.HomePageContentPresenter.Content = new MainPageFileExplorer();
                    break;
                case VLCPage.MainPageNetwork:
                    HomePageController.HomePageContentPresenter.Content = new MainPageNetwork();
                    break;
            }
        }
    }
}
