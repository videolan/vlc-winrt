using System;
using System.Windows.Input;

namespace VLC_WINRT.Utility.Commands
{
    public class SkipBackCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return false;
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        public event EventHandler CanExecuteChanged;
    }
}