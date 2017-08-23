using System;
using System.Diagnostics;
using VLC.Helpers;
using VLC.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace VLC.Universal8._1.Views.MainPages
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
