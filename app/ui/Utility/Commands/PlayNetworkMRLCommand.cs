using System;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.Views;

namespace VLC_WINRT.Utility.Commands
{
    public class PlayNetworkMRLCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            var mrl = parameter as string;
            if (string.IsNullOrEmpty(mrl))
                throw new ArgumentException("Expecting to see a string mrl for this command");

            Locator.PlayVideoVM.SetActiveVideoInfo(mrl);
            NavigationService.NavigateTo(new PlayVideo());
        }
    }
}