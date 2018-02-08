using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using VLC.Helpers;
using VLC.ViewModels;

namespace VLC.UI.Views.MainPages
{
    public sealed partial class MainPageXBOX
    {
        public MainPageXBOX()
        {
            this.InitializeComponent();
            PanelsListView.Focus(Windows.UI.Xaml.FocusState.Keyboard);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await AppViewHelper.SetAppView(true);
            App.SplitShell.FlyoutAsHeader = true;
            base.OnNavigatedTo(e);
            if (Locator.SettingsVM.ExtraMargin)
            {
                var extraMargin = Locator.SettingsVM.ExtraMarginValue;
                Margin = new Thickness(Margin.Left + extraMargin, Margin.Top + extraMargin, + Margin.Right + extraMargin, Margin.Bottom + extraMargin);
            }
            PanelsListView.ItemsSource = Locator.MainVM.Panels;
            PanelsListView.SelectedItem = Locator.MainVM.CurrentPanel;
            
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            App.SplitShell.FlyoutAsHeader = false;
            base.OnNavigatingFrom(e);
        }
        
        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Locator.MainVM.CurrentPanel = e.ClickedItem as Model.Panel;
        }

        private void ItemsWrapGrid_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (PanelsListView.SelectedIndex == -1)
                return;

            ((PanelsListView.ItemsPanelRoot as ItemsWrapGrid).Children[PanelsListView.SelectedIndex] as ListViewItem).Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }
    }
}
