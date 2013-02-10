using System;
using System.Windows.Input;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Utility.Commands
{
    internal class PlayCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            ViewModelLocator.PlayVideoVM.VLCPlayer.Play();
        }

        public event EventHandler CanExecuteChanged;
    }
}