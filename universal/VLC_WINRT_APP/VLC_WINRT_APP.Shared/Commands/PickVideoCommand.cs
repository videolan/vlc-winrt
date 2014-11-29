/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using VLC_WINRT_APP.Model;
using System.Diagnostics;
using VLC_WINRT_APP.Services.RunTime;

namespace VLC_WINRT_APP.Commands
{
    public class PickVideoCommand : ICommand
    {
        private static readonly object Locker = new object();
        private bool _canExecute = true;

        public bool CanExecute(object parameter)
        {
            lock (Locker)
            {
                return _canExecute;
            }
        }

        public async void Execute(object parameter)
        {
            //lock (Locker)
            //{
            //    _canExecute = false;
            //    CanExecuteChanged(this, new EventArgs());
            //}

            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            };
            foreach (string videoExtension in VLCFileExtensions.VideoExtensions)
            {
                picker.FileTypeFilter.Add(videoExtension);
            }
            foreach (string audioExtension in VLCFileExtensions.AudioExtensions)
            {
                picker.FileTypeFilter.Add(audioExtension);
            }

#if WINDOWS_APP
            StorageFile file = null;
            file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Debug.WriteLine("Opening file: " + file.Path);
                await MediaService.PlayVideoFile(file);
            }
            else
            {
                Debug.WriteLine("Cancelled");
            }
#else
            App.OpenFilePickerReason = OpenFilePickerReason.OnOpeningVideo;
            picker.PickSingleFileAndContinue();
#endif
            //lock (Locker)
            //{
            //    _canExecute = true;
            //    CanExecuteChanged(this, new EventArgs());
            //}
        }
        public event EventHandler CanExecuteChanged;
    }
}
