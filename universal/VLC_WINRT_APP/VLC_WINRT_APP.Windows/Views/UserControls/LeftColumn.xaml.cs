using System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class LeftColumn : UserControl
    {
        public LeftColumn()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += CurrentOnSizeChanged;
        }

        private void CurrentOnSizeChanged(object sender, WindowSizeChangedEventArgs windowSizeChangedEventArgs)
        {
            if (Window.Current.Bounds.Width < 1080)
            {
                ColumnGrid.Width = 100;
                ToMediumVisualState();
            }
            else
            {
                ColumnGrid.Width = 340;
                ToNormalVisualState();
            }
        }

        void ToMediumVisualState()
        {
            TitleTextBlock.Visibility = Visibility.Collapsed;
            LargeSearchBox.Visibility = Visibility.Collapsed;
            LittleSearchBox.Visibility = Visibility.Visible;

            HeaderGrid.Margin = new Thickness(0);
            HeaderGrid.HorizontalAlignment = HorizontalAlignment.Center;

            PanelsListView.ItemTemplate = App.Current.Resources["SidebarIconItemTemplate"] as DataTemplate;
        }

        void ToNormalVisualState()
        {
            TitleTextBlock.Visibility = Visibility.Visible;
            LargeSearchBox.Visibility = Visibility.Visible;
            LittleSearchBox.Visibility = Visibility.Collapsed;

            HeaderGrid.Margin = new Thickness(42,0,20,0);
            HeaderGrid.HorizontalAlignment = HorizontalAlignment.Left;
            PanelsListView.ItemTemplate = App.Current.Resources["SidebarItemTemplate"] as DataTemplate;
        }

        private void MainPanels_ItemClick(object sender, ItemClickEventArgs e)
        {
            Model.Panel panel = e.ClickedItem as Model.Panel;
            foreach (Model.Panel panel1 in Locator.MainVM.Panels)
                panel1.Opacity = 0.4;
            panel.Opacity = 1;
            switch (panel.Index)
            {
                case 0:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageHome))
                        App.ApplicationFrame.Navigate(typeof(MainPageHome));
                    break;
                case 1:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageVideos))
                        App.ApplicationFrame.Navigate(typeof(MainPageVideos));
                    break;
                case 2:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageMusic))
                        App.ApplicationFrame.Navigate(typeof(MainPageMusic));
                    break;
                case 3:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof (MainPageRemovables))
                        App.ApplicationFrame.Navigate(typeof (MainPageRemovables));
                    break;
                case 4:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof (MainPageMediaServers))
                        App.ApplicationFrame.Navigate(typeof (MainPageMediaServers));
                    break;
            }
        }
    }
}
