using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using VLC.ViewModels;
using Windows.UI.Xaml.Input;
using VLC.Commands.VLCFileExplorer;
using VLC.Model;
using VLC.Model.FileExplorer;
using VLC.Universal8._1.Views.UserControls.Flyouts;
using VLC.Commands;
using VLC.Utils;
using Windows.System;
using VLC.Commands.MediaLibrary;

namespace VLC.Universal8._1.Views.MainPages
{
    public sealed partial class MainPageFileExplorer : Page
    {
        public MainPageFileExplorer()
        {
            this.InitializeComponent();
            this.Loaded += MainPageFileExplorer_Loaded;
        }

        private async void MainPageFileExplorer_Loaded(object sender, RoutedEventArgs e)
        {
            await Locator.FileExplorerVM.OnNavigatedTo();
        }
    }
}