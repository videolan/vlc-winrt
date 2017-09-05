using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;
using VLC;
using VLC.ViewModels;

namespace VLC_WinRT.UI.Legacy.Views.MainPages
{
    public sealed partial class MainPageNetwork : Page
    {
        public MainPageNetwork()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CoreWindow.GetForCurrentThread().KeyDown += KeyboardListenerService_KeyDown;
            Locator.StreamsVM.OnNavigatedTo();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            CoreWindow.GetForCurrentThread().KeyDown -= KeyboardListenerService_KeyDown;
            Locator.StreamsVM.OnNavigatedFrom();
        }

        private void KeyboardListenerService_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Enter)
            {
                Locator.StreamsVM.PlayStreamCommand.Execute(MrlTextBox.Text);
            }
        }

        private void MrlTextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            MrlTextBox.Foreground = App.Current.Resources["MainColor"] as SolidColorBrush;
        }
    }
}