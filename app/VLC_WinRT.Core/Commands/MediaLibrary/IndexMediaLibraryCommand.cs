using System;
using VLC_WinRT.Utils;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.MediaLibrary
{
    public class IndexMediaLibraryCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            if (parameter == null && parameter.ToString() == "True")
            {
                await Locator.MediaLibrary.Initialize();
            }
        }
    }
}
