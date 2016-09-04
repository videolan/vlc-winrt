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
using VLC.Commands.MediaLibrary;

namespace VLC.Views.MainPages
{
    public sealed partial class MainPageFileExplorer : Page
    {
        private ListViewItem focussedListViewItem;

        public MainPageFileExplorer()
        {
            this.InitializeComponent();
            this.Loaded += MainPageFileExplorer_Loaded;
        }

        private async void MainPageFileExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            Responsive();
            this.SizeChanged += OnSizeChanged;
            this.Unloaded += OnUnloaded;
            await Locator.FileExplorerVM.OnNavigatedTo();
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