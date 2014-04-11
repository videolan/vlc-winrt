/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.Storage.AccessCache;
using Autofac;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Pickers;
using VLC_WINRT.Utility.Services.RunTime;
using VLC_WINRT.ViewModels;
using VLC_WINRT.ViewModels.MainPage;

#if WINDOWS_PHONE_APP
using VLC_WINPRT;
#endif
#if NETFX_CORE
using VLC_WINRT.Views;
#endif

namespace VLC_WINRT.Utility.Commands
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
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.VideosLibrary
            };

            //TODO: add more supported types
            picker.FileTypeFilter.Add(".3g2" );
            picker.FileTypeFilter.Add(".3gp" );
            picker.FileTypeFilter.Add(".3gp2");
            picker.FileTypeFilter.Add(".3gpp");
            picker.FileTypeFilter.Add(".amv" );
            picker.FileTypeFilter.Add(".asf" );
            picker.FileTypeFilter.Add(".avi" );
            picker.FileTypeFilter.Add(".divx");
            picker.FileTypeFilter.Add(".drc" );
            picker.FileTypeFilter.Add(".dv"  );
            picker.FileTypeFilter.Add(".f4v" );
            picker.FileTypeFilter.Add(".flv" );
            picker.FileTypeFilter.Add(".gvi" );
            picker.FileTypeFilter.Add(".gxf" );
            picker.FileTypeFilter.Add(".ismv");
            picker.FileTypeFilter.Add(".iso" );
            picker.FileTypeFilter.Add(".m1v" );
            picker.FileTypeFilter.Add(".m2v" );
            picker.FileTypeFilter.Add(".m2t" );
            picker.FileTypeFilter.Add(".m2ts");
            picker.FileTypeFilter.Add(".m3u8");
            picker.FileTypeFilter.Add(".mkv" );
            picker.FileTypeFilter.Add(".mov" );
            picker.FileTypeFilter.Add(".mp2" );
            picker.FileTypeFilter.Add(".mp2v");
            picker.FileTypeFilter.Add(".mp4" );
            picker.FileTypeFilter.Add(".mp4v");
            picker.FileTypeFilter.Add(".mpe" );
            picker.FileTypeFilter.Add(".mpeg");
            picker.FileTypeFilter.Add(".mpeg1");
            picker.FileTypeFilter.Add(".mpeg2");
            picker.FileTypeFilter.Add(".mpeg4");
            picker.FileTypeFilter.Add(".mpg" );
            picker.FileTypeFilter.Add(".mpv2");
            picker.FileTypeFilter.Add(".mts" );
            picker.FileTypeFilter.Add(".mtv" );
            picker.FileTypeFilter.Add(".mxf" );
            picker.FileTypeFilter.Add(".mxg" );
            picker.FileTypeFilter.Add(".nsv" );
            picker.FileTypeFilter.Add(".nut" );
            picker.FileTypeFilter.Add(".nuv" );
            picker.FileTypeFilter.Add(".ogm" );
            picker.FileTypeFilter.Add(".ogv" );
            picker.FileTypeFilter.Add(".ogx" );
            picker.FileTypeFilter.Add(".ps"  );
            picker.FileTypeFilter.Add(".rec" );
            picker.FileTypeFilter.Add(".rm"  );
            picker.FileTypeFilter.Add(".rmvb");
            picker.FileTypeFilter.Add(".tod" );
            picker.FileTypeFilter.Add(".ts"  );
            picker.FileTypeFilter.Add(".tts" );
            picker.FileTypeFilter.Add(".vob" );
            picker.FileTypeFilter.Add(".vro" );
            picker.FileTypeFilter.Add(".webm");
            picker.FileTypeFilter.Add(".wm");
            picker.FileTypeFilter.Add(".wmv" );
            picker.FileTypeFilter.Add(".wtv" );
            picker.FileTypeFilter.Add(".xesc");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                Debug.WriteLine("Opening file: " + file.Path);
                var video = new MediaViewModel();
                video.Initialize(file);
                await video.Initialize();
                Locator.PlayVideoVM.CurrentVideo = video;
                Locator.PlayVideoVM.SetActiveVideoInfo(video.Token, file.Name);
#if NETFX_CORE
                NavigationService.NavigateTo(typeof(PlayVideo));
#endif
            }
            else
            {
                Debug.WriteLine("Cancelled");
            }

            //lock (Locker)
            //{
            //    _canExecute = true;
            //    CanExecuteChanged(this, new EventArgs());
            //}
        }

        public event EventHandler CanExecuteChanged;
    }
}
