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
using Windows.Storage.Pickers;
using VLC_WinRT.Model;
using System.Collections.Generic;
using Windows.Storage;
using VLC_WinRT.Helpers;
using VLC_WinRT.ViewModels;

namespace VLC_WinRT.Commands.VideoLibrary
{
    public class PickVideoCommand : ICommand
    {
        private static readonly object Locker = new object();
        private bool _canExecute = true;

        static List<String> _allowedExtensions = new List<string>();
        static PickVideoCommand()
        {
            foreach (string videoExtension in VLCFileExtensions.VideoExtensions)
            {
                _allowedExtensions.Add(videoExtension);
            }
            foreach (string audioExtension in VLCFileExtensions.AudioExtensions)
            {
                _allowedExtensions.Add(audioExtension);
            }
        }

        public bool CanExecute(object parameter)
        {
            lock (Locker)
            {
                return _canExecute;
            }
        }

        public async void Execute(object parameter)
        {
            try
            {
                App.OpenFilePickerReason = OpenFilePickerReason.OnOpeningVideo;
                var picker = new FileOpenPicker
                {
                    ViewMode = PickerViewMode.List,
                    SuggestedStartLocation = PickerLocationId.VideosLibrary
                };
                foreach (var ext in _allowedExtensions)
                    picker.FileTypeFilter.Add(ext);


#if WINDOWS_PHONE_APP                
                picker.PickSingleFileAndContinue();
#else
                StorageFile file = null;
                file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    LogHelper.Log("Opening file: " + file.Path);
                    await Locator.MediaPlaybackViewModel.OpenFile(file);
                }
                else
                {
                    LogHelper.Log("Cancelled");
                }
                App.OpenFilePickerReason = OpenFilePickerReason.Null;
#endif
            }
            catch { }
        }
        public event EventHandler CanExecuteChanged;
    }
}
