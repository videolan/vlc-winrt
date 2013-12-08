using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Commands
{
    public class ClearHistoryCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var historyService = IoC.IoC.GetInstance<HistoryService>();
            LastViewedViewModel lastViewedVM = Locator.MainPageVM.LastViewedVM;
            historyService.Clear();
            lastViewedVM.LastViewedVM = null;
            lastViewedVM.SecondLastViewedVM = null;
            lastViewedVM.ThirdLastViewedVM = null;
            lastViewedVM.LastViewedSectionVisible = false;
            lastViewedVM.WelcomeSectionVisibile = true;

        }
    }
}