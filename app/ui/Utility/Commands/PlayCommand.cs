using System;
using VLC_WINRT.Common;
using VLC_WINRT.ViewModels.MainPage;
using VLC_WINRT.Views;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace VLC_WINRT.Utility.Commands
{
    public class PlayCommand : AlwaysExecutableCommand
    {
        public override void Execute(object parameter)
        {
            if (parameter.GetType() != typeof(MediaViewModel))
                throw new ArgumentException("Expecting to see a Media View Model for this command");

            var vm = (MediaViewModel) parameter;
            PlayVideo.CurrentFile = vm.File;
            ((Frame) Window.Current.Content).Navigate(typeof (PlayVideo));
        }
    }
}