using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Xaml.Interactivity;
using VLC.ViewModels;
using Windows.UI.Xaml.Input;
using VLC.Commands.VLCFileExplorer;
using VLC.Model;
using VLC.Model.FileExplorer;
using VLC.Views.UserControls.Flyouts;
using VLC.Commands;
using VLC.Utils;
using Windows.System;

namespace VLC.Views.MainPages
{
    public sealed partial class MainPageFileExplorer : Page
    {
        private ListViewItem focussedListViewItem;

        public MainPageFileExplorer()
        {
            this.InitializeComponent();
            this.Loaded += MainPageFileExplorer_Loaded;

            StorageItemsListView.KeyDown += StorageItemsListView_KeyDown;
            StorageItemsListView.GotFocus += StorageItemsListView_GotFocus;
        }
        
        private async void MainPageFileExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
            this.SizeChanged += OnSizeChanged;
            this.Unloaded += OnUnloaded;
            await Locator.FileExplorerVM.OnNavigatedTo();
        }

        private void StorageItemsListView_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (Locator.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer
                && e.Key == VirtualKey.GamepadView)
            {
                var menu = new MenuFlyout();

                menu.Items.Add(new MenuFlyoutItem()
                {
                    Name = "PlayPauseItem",
                    Text = Strings.CopyToLocalStorage,
                    Command = new ActionCommand(() => Locator.MediaPlaybackViewModel.PlaybackService.Pause())
                });
                focussedListViewItem.ContextFlyout = menu;
                focussedListViewItem.ContextFlyout.ShowAt(focussedListViewItem);
            }
        }

        void StorageItemsListView_GotFocus(object sender, RoutedEventArgs e)
        {
            ListView list = (ListView)sender;
            if (FocusManager.GetFocusedElement() as ListViewItem != null)
            {
                focussedListViewItem = (ListViewItem)FocusManager.GetFocusedElement();
            }
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }

        private void Responsive()
        {
            if (Window.Current.Bounds.Width < 600)
            {
                if (Window.Current.Bounds.Width < 550)
                {
                    OpenFileButton.IsCompact = GoBackButton.IsCompact = true;
                }
            }
            else
            {
                OpenFileButton.IsCompact = GoBackButton.IsCompact = false;
            }
        }
    }
}