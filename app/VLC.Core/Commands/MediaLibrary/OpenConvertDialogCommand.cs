using System;
using VLC.Model;
using VLC.Universal.Views.UserControls.Shell;
using VLC.Utils;

namespace VLC.Commands.MediaLibrary
{
    public class OpenConvertDialogCommand : AlwaysExecutableCommand
    {
        public override async void Execute(object parameter)
        {
            if (parameter is IMediaItem)
            {
                var media = parameter as IMediaItem;

                var dialog = new TranscodingDialog
                {
                    Media = media
                };
                
                await dialog.ShowAsync();
            }
        }
    }
}