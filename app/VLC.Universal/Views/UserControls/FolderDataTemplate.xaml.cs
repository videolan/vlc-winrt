using System;
using VLC.Model.FileExplorer;
using VLC.ViewModels;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WinRTXamlToolkit.Controls.Extensions;

namespace VLC.UI.Views.UserControls
{
    public sealed partial class FolderDataTemplate : UserControl
    {
        public FolderDataTemplate()
        {
            this.InitializeComponent();
            this.Loaded += FolderDataTemplate_Loaded;
            PointerEntered += OnPointerEntered;
            PointerExited += OnPointerExited;
            Unloaded += OnUnloaded;
        }

        void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            PointerEntered -= OnPointerEntered;
            PointerExited -= OnPointerExited;
        }

        void OnPointerExited(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            StopAutoScroll();
        }

        void OnPointerEntered(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            StartAutoScroll();
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
            StopAutoScroll();
        }

        private void KeyboardListenerService_KeyDownPressed(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            switch (args.VirtualKey)
            {
                case VirtualKey.GamepadView:
                    ShowFlyout();
                    break;
                case VirtualKey.GamepadY:
                    if ((this.DataContext as IVLCStorageItem).StorageItem != null)
                        Locator.FileExplorerVM.CurrentStorageVM.CopyCommand.Execute(this.DataContext);
                    break;
                default:
                    break;
            }
        }

        private void FolderDataTemplate_GotFocus(object sender, RoutedEventArgs e)
        {
            Locator.MainVM.KeyboardListenerService.KeyDownPressed += KeyboardListenerService_KeyDownPressed;
            StartAutoScroll();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowFlyout();
        }

        void ShowFlyout()
        {
            if ((this.DataContext as IVLCStorageItem).StorageItem != null)
                Flyout.ShowAttachedFlyout(RootGrid);
        }

        public void StopAutoScroll()
        {
            CompositionTarget.Rendering -= CompositionTargetOnRendering;
            scrollviewer.ChangeView(0, null, null, false);
        }

        public void StartAutoScroll()
        {
            CompositionTarget.Rendering += CompositionTargetOnRendering;
        }

        void CompositionTargetOnRendering(object sender, object o)
        {
            scrollviewer.ChangeView(scrollviewer.HorizontalOffset + 1, null, null, false);
        }
    }
}
