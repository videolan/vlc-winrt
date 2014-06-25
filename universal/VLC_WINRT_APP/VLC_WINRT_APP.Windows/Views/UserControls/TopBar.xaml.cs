using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.Views.MainPages;

namespace VLC_WINRT_APP.Views.UserControls
{
    public sealed partial class TopBar : UserControl
    {
        public TopBar()
        {
            this.InitializeComponent();
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
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageRemovables))
                        App.ApplicationFrame.Navigate(typeof(MainPageRemovables));
                    break;
                case 4:
                    if (App.ApplicationFrame.CurrentSourcePageType != typeof(MainPageMediaServers))
                        App.ApplicationFrame.Navigate(typeof(MainPageMediaServers));
                    break;
            }
        }
    }
}
