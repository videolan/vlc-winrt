using System.Diagnostics;
using VLC.Helpers;
using VLC.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class FolderDataTemplate : UserControl
    {
        public FolderDataTemplate()
        {
            this.InitializeComponent();
            this.Loaded += FolderDataTemplate_Loaded;
        }

        private void FolderDataTemplate_Loaded(object sender, RoutedEventArgs e)
        {
            var listViewItem = this.GetFirstAncestorOfType<ListViewItem>();
            if (listViewItem != null)
            {
                listViewItem.GotFocus += FolderDataTemplate_GotFocus;
                listViewItem.LostFocus += ListViewItem_LostFocus;
            }
        }

        private void ListViewItem_LostFocus(object sender, RoutedEventArgs e)
        {
            Locator.MainVM.KeyboardListenerService.KeyDownPressed -= KeyboardListenerService_KeyDownPressed;
        }

        private void KeyboardListenerService_KeyDownPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if (args.VirtualKey == Windows.System.VirtualKey.GamepadView)
            {
                ShowFlyout();
            }
        }

        private void FolderDataTemplate_GotFocus(object sender, RoutedEventArgs e)
        {
            Locator.MainVM.KeyboardListenerService.KeyDownPressed += KeyboardListenerService_KeyDownPressed;
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowFlyout();
        }

        void ShowFlyout()
        {
            Flyout.ShowAttachedFlyout(RootGrid);
        }
    }
}
