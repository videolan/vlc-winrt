using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;

namespace VLC_WinRT.Views.UserControls
{
    public sealed partial class ShellContent : UserControl
    {
        public int CurrentViewIndex;
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
            if(Locator.NavigationService.IsCurrentPageAMainPage())
                FlipViewFrameContainer.IsLocked = false;
            else FlipViewFrameContainer.IsLocked = true;
        }

        private async void FlipViewFrameContainerOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            var index = FlipViewFrameContainer.SelectedIndex;
            if (index == 1) return;
            await Task.Delay(200);
            FlipViewFrameContainer.SelectedIndex = 1;
            EntranceThemeTransition.FromVerticalOffset = 0;
            var newCurrentIndex = 0;
            if (index == 0)
            {
                SetPivotAnimation(false);
                if (Locator.NavigationService.CurrentPage == VLCPage.MainPageVideo)
                    newCurrentIndex = 3;
                else if (Locator.NavigationService.CurrentPage == VLCPage.MainPageMusic)
                    newCurrentIndex = 0;
                else if (Locator.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer)
                    newCurrentIndex = 1;
                else if (Locator.NavigationService.CurrentPage == VLCPage.MainPageNetwork)
                    newCurrentIndex = 2;
            }
            else if (index == 2)
            {
                SetPivotAnimation(true);
                if (Locator.NavigationService.CurrentPage == VLCPage.MainPageVideo)
                    newCurrentIndex = 1;
                else if (Locator.NavigationService.CurrentPage == VLCPage.MainPageMusic)
                    newCurrentIndex = 2;
                else if (Locator.NavigationService.CurrentPage == VLCPage.MainPageFileExplorer)
                    newCurrentIndex = 3;
                else if (Locator.NavigationService.CurrentPage == VLCPage.MainPageNetwork)
                    newCurrentIndex = 0;
                // Told ya ¯\_(ツ)_/¯
            }
            Locator.MainVM.CurrentPanel = Locator.MainVM.Panels[newCurrentIndex];
            await Task.Delay(200);
            CurrentViewIndex = newCurrentIndex;
        }

        public void SetPivotAnimation(bool isNextPivot)
        {
            if (isNextPivot)
            {
                EntranceThemeTransition.FromHorizontalOffset = 200;
            }
            else
            {

                EntranceThemeTransition.FromHorizontalOffset = -200;
            }
        }
    }
}
