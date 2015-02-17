using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace VLC_WINRT_APP.Views.MainPages.MainVideoControls
{
    public sealed partial class ShowsPivotItem : Page
    {
        public ShowsPivotItem()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void ListView_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = sender as ListView;
#if WINDOWS_PHONE_APP
            var xaml = this.Resources["WindowsPhonePanelTemplate"];
            lv.ItemsPanel = xaml as ItemsPanelTemplate;
#else
            var xaml = this.Resources["WindowsPanelTemplate"];
            lv.ItemsPanel = xaml as ItemsPanelTemplate;
#endif
        }
    }
}
