using System;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace VLC_WINRT_APP.Views.UserControls.Placeholder
{
    public sealed partial class NoData : UserControl
    {
        public NoData()
        {
            this.InitializeComponent();
        }

        private async void RootGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var md = new MessageDialog("We're not able to download any music information from the Internet", "Sorry");
            await md.ShowAsync().AsTask();
        }
    }
}
