/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Autofac;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif

namespace VLC_WINRT.Utility.Commands
{
    public class ClearHistoryCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var historyService = App.Container.Resolve<HistoryService>();
            LastViewedViewModel lastViewedVM = Locator.MainPageVM.LastViewedVM;
            historyService.Clear();
            lastViewedVM.LastViewedVM = null;
            lastViewedVM.LastViewedSectionVisible = false;
            lastViewedVM.WelcomeSectionVisible = true;

        }
    }
}
