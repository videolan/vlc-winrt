using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using VLC.ViewModels;
using Windows.UI.Xaml.Media;
using VLC.Commands.StreamsLibrary;
using VLC.Utils;

namespace VLC.UI.Views.MainPages
{
    public sealed partial class MainPageNetwork : Page
    {
        private ListViewItem _focusedElement;

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
            else if (args.VirtualKey == VirtualKey.GamepadView && _focusedElement != null)
            {
                var flyout = new MenuFlyout
                {
                    Items =
                    {
                        new MenuFlyoutItem
                        {
                            Text = Strings.DeleteSelected,
                            Command = new DeleteStreamCommand(),
                            CommandParameter = _focusedElement.Content
                        }
                    }
                };
                flyout.ShowAt(_focusedElement);
            }
        }

        private void MrlTextBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            MrlTextBox.Foreground = App.Current.Resources["MainColor"] as SolidColorBrush;
        }


        private void NetworkItemGotFocus(object sender, RoutedEventArgs e)
        {
            if (FocusManager.GetFocusedElement() is ListViewItem)
            {
                _focusedElement = (ListViewItem)FocusManager.GetFocusedElement();
            }
        }
    }
}