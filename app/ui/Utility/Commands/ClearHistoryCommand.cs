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
            var histserv = new HistoryService();
            LastViewedViewModel lastViewedVM = ViewModelLocator.MainPageVM.LastViewedVM;
            histserv.Clear();
            lastViewedVM.LastViewedVM = null;
            lastViewedVM.SecondLastViewedVM = null;
            lastViewedVM.ThirdLastViewedVM = null;
            lastViewedVM.LastViewedSectionVisible = false;
            lastViewedVM.WelcomeSectionVisibile = true;

        }
    }
}