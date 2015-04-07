using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Views.MainPages;
using VLC_WinRT.Model;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class ShellContent : UserControl
    {
        public ShellContent()
        {
            this.InitializeComponent();
        }

        // /!\          WARNING         /!\
        // /!\ Don't look at this crazy /!\
        // /!\ workaround, this is crap /!\
        // /!\       Please don't ..    /!\
        private void FlipViewFrameContainer_OnLoaded(object sender, RoutedEventArgs e)
        {
            FlipViewFrameContainer.SelectedIndex = 1;
            FlipViewFrameContainer.SelectionChanged += FlipViewFrameContainerOnSelectionChanged;
            App.ApplicationFrame.Navigated += ApplicationFrame_Navigated;
        }

        void ApplicationFrame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            if(Locator.MainVM.NavigationService.IsCurrentPageAMainPage())
                FlipViewFrameContainer.IsLocked = false;
            else FlipViewFrameContainer.IsLocked = true;
        }

        private async void FlipViewFrameContainerOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var index = FlipViewFrameContainer.SelectedIndex;
            await Task.Delay(200);
            FlipViewFrameContainer.SelectedIndex = 1;
            if (index == 0)
            {
                if(Locator.MainVM.NavigationService.CurrentPage == VLCPage.MainPageHome)
                    Locator.MainVM.GoToPanelCommand.Execute(3);
                else if (Locator.MainVM.NavigationService.CurrentPage == VLCPage.MainPageVideo)
                    Locator.MainVM.GoToPanelCommand.Execute(0);
                else if (Locator.MainVM.NavigationService.CurrentPage == VLCPage.MainPageMusic)
                    Locator.MainVM.GoToPanelCommand.Execute(1);
                else if (Locator.MainVM.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer)
                    Locator.MainVM.GoToPanelCommand.Execute(2);
            }
            else if (index == 2)
            {
                if (Locator.MainVM.NavigationService.CurrentPage == VLCPage.MainPageHome)
                    Locator.MainVM.GoToPanelCommand.Execute(1);
                else if (Locator.MainVM.NavigationService.CurrentPage == VLCPage.MainPageVideo)
                    Locator.MainVM.GoToPanelCommand.Execute(2);
                else if (Locator.MainVM.NavigationService.CurrentPage == VLCPage.MainPageMusic)
                    Locator.MainVM.GoToPanelCommand.Execute(3);
                else if (Locator.MainVM.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer)
                    Locator.MainVM.GoToPanelCommand.Execute(0);
                // Told ya ¯\_(ツ)_/¯
            }
        }
    }
}
