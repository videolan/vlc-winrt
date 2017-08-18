using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VLC.ViewModels.Others;

namespace VLC.UI.Views.MainPages
{
    public sealed partial class MainPageNetwork : StreamsPage
    {
        public MainPageNetwork()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CoreWindow.GetForCurrentThread().KeyDown += KeyboardListenerService_KeyDown;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            CoreWindow.GetForCurrentThread().KeyDown -= KeyboardListenerService_KeyDown;
        }

        private void KeyboardListenerService_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Enter)
            {
                ViewModel.PlayStreamCommand.Execute(MrlTextBox.Text);
            }
        }

        private void MrlTextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            MrlTextBox.Foreground = App.Current.Resources["MainColor"] as SolidColorBrush;
        }
    }

    /// <summary>
    /// Mandatory intermediate class as partial page classes inheriting generic abstract classes do not play well with XAML
    /// </summary>
    public class StreamsPage : VlcPage<StreamsViewModel>
    {
    }
}