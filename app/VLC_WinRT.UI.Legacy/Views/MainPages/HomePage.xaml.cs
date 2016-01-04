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
            Locator.MainVM.PropertyChanged += MainVM_PropertyChanged;
            Navigate(Locator.MainVM.CurrentPanel.Target);
            this.SizeChanged += HomePage_SizeChanged;
#if WINDOWS_PHONE_APP
            VisualStateUtilities.GoToState(this, nameof(Phone), false);
#elif WINDOWS_APP
            VisualStateUtilities.GoToState(this, nameof(Standard), false);
#endif
        }

        private void HomePage_SizeChanged(object sender, Windows.UI.Xaml.SizeChangedEventArgs e)
        {
            Responsive();
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
                    if (HomePageController.HomePageContentPresenter.Content is MainPageVideos) return;
                    HomePageController.HomePageContentPresenter.Content = new MainPageVideos();
                    break;
                case VLCPage.MainPageMusic:
                    if (HomePageController.HomePageContentPresenter.Content is MainPageMusic) return;
                    HomePageController.HomePageContentPresenter.Content = new MainPageMusic();
                    break;
                case VLCPage.MainPageFileExplorer:
                    if (HomePageController.HomePageContentPresenter.Content is MainPageFileExplorer) return;
                    HomePageController.HomePageContentPresenter.Content = new MainPageFileExplorer();
                    break;
                case VLCPage.MainPageNetwork:
                    if (HomePageController.HomePageContentPresenter.Content is MainPageNetwork) return;
                    HomePageController.HomePageContentPresenter.Content = new MainPageNetwork();
                    break;
            }
        }
        
        void Responsive()
        {
        }
    }
}
