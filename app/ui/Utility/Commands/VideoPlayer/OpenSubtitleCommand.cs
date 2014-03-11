using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using VLC_WINRT.Common;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;

namespace VLC_WINRT.Utility.Commands.VideoPlayer
{
    public class OpenSubtitleCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            };
            picker.FileTypeFilter.Add(".srt");
            picker.FileTypeFilter.Add(".ass");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string mru = StorageApplicationPermissions.FutureAccessList.Add(file);

                string mrl = "file://" + mru;
                Locator.PlayVideoVM.OpenSubtitle(mrl);
            }
            else
            {
                Debug.WriteLine("Cancelled Opening subtitle");
            }
        }
    }
}
