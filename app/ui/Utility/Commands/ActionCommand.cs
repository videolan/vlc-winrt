using System;
using VLC_WINRT.Common;

namespace VLC_WINRT.Utility.Commands
{
    public class ActionCommand : AlwaysExecutableCommand
    {
        private readonly Action _action;
        public ActionCommand(Action action)
        {
            _action = action;
        }

        public override void Execute(object parameter)
        {
            _action();
        }
    }
}