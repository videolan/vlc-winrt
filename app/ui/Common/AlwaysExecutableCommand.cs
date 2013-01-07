using System;
using System.Windows.Input;

namespace VLC_WINRT.Common
{
    public abstract class AlwaysExecutableCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public abstract void Execute(object parameter);

        public event EventHandler CanExecuteChanged;
    }
}