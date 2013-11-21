using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Views;

namespace VLC_WINRT.Utility.Commands
{
    public class OpenFileCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter.GetType() != typeof (MediaViewModel) && parameter.GetType() != typeof (ViewedVideoViewModel))
                throw new ArgumentException("Expecting to see a Media View Model for this command");

            var history = new HistoryService();
            var vm = (MediaViewModel) parameter;

            ViewModelLocator.PlayVideoVM.CurrentFile = vm.File;
            history.Add(vm.File);
            ((Frame) Window.Current.Content).Navigate(typeof (PlayVideo));
        }
    }
}