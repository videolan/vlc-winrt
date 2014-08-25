using System;
using System.Collections.Generic;
using System.Text;
using VLC_WINRT.Common;
using VLC_WINRT.Views;
using VLC_WINRT_APP.Views.VariousPages;

namespace VLC_WINRT_APP.Commands.MainPageCommands
{
    public class GoToThanksPageCommand: AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (App.ApplicationFrame.CurrentSourcePageType != typeof (SpecialThanks))
            {
                App.ApplicationFrame.Navigate(typeof (SpecialThanks));
            }
        }
    }
}
