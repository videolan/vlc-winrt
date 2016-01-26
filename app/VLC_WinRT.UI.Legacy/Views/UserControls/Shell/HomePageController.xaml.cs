using VLC_WinRT.Model;
using VLC_WinRT.UI.Legacy.Views.MainPages;
using VLC_WinRT.UI.Legacy.Views.VariousPages;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;
using Windows.UI.Xaml.Controls;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class HomePageController : UserControl
    {
        public VLCPage CurrentHomePage
        {
            get; private set;
        }

        public HomePageController()
        {
            this.InitializeComponent();
            this.Loaded += HomePageController_Loaded;
        }

        public bool IsPivotItem
        {
            get
            {
                return this.Tag?.ToString() == "Pivot";
            }
        }

        private void HomePageController_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.Unloaded += HomePageController_Unloaded;
            Locator.MainVM.PropertyChanged += MainVM_PropertyChanged;
            Navigate(Locator.MainVM.CurrentPanel.Target);
        }

        private void HomePageController_Unloaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Locator.MainVM.PropertyChanged -= MainVM_PropertyChanged;
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
            if (IsPivotItem)
            {
                var panel = (Model.Panel)this.DataContext;
                if (panel.Target != page)
                {
                    HomePageContentPresenter.Content = null;
                    return;
                }
            }
            CurrentHomePage = page;
            switch (page)
            {
                case VLCPage.MainPageVideo:
                    if (HomePageContentPresenter.Content is MainPageVideos) return;
                    HomePageContentPresenter.Navigate(typeof(MainPageVideos));
                    break;
                case VLCPage.MainPageMusic:
                    if (HomePageContentPresenter.Content is MainPageMusic) return;
                    HomePageContentPresenter.Navigate(typeof(MainPageMusic));
                    break;
                case VLCPage.MainPageFileExplorer:
                    if (HomePageContentPresenter.Content is MainPageFileExplorer) return;
                    HomePageContentPresenter.Navigate(typeof(MainPageFileExplorer));
                    break;
                case VLCPage.MainPageNetwork:
                    if (HomePageContentPresenter.Content is MainPageNetwork) return;
                    HomePageContentPresenter.Navigate(typeof(MainPageNetwork));
                    break;
                case VLCPage.SearchPage:
                    if (HomePageContentPresenter.Content is SearchPage) return;
                    HomePageContentPresenter.Navigate(typeof(SearchPage));
                    break;
            }
        }
    }
}