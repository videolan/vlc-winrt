using System;
using System.Diagnostics;
using VLC_WinRT.Helpers;
using VLC_WinRT.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace VLC_WinRT.UI.Legacy.Views.MainPages
{
    public sealed partial class MainPageXBOX
    {
        public MainPageXBOX()
        {
            this.InitializeComponent();
            AppViewHelper.SetAppView(true);
            PanelsListView.Focus(Windows.UI.Xaml.FocusState.Keyboard);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.SplitShell.FlyoutAsHeader = true;
            base.OnNavigatedTo(e);
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
            var model = e.ClickedItem as Model.Panel;
            Locator.NavigationService.Go(model.Target);
        }

        private void ItemsWrapGrid_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ((PanelsListView.ItemsPanelRoot as ItemsWrapGrid).Children[PanelsListView.SelectedIndex] as ListViewItem).Focus(Windows.UI.Xaml.FocusState.Programmatic);
        }
    }
}
