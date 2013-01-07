using System;
using VLC_WINRT.Common;
using VLC_WINRT.ViewModels.MainPage;

namespace VLC_WINRT.Utility.Commands
{
    public class PlayVideoCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter.GetType() != typeof(MediaViewModel))
                throw new ArgumentException("Expecting to see a Media View Model for this command");

            var vm = (MediaViewModel) parameter;
            
        }
    }
}