/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using VLC_WINRT.Common;
using VLC_WINRT_APP.Common;
using VLC_WINRT_APP.Helpers.MusicLibrary;
using VLC_WINRT_APP.Model;
using VLC_WINRT_APP.Model.Music;
using VLC_WINRT_APP.ViewModels;
using VLC_WINRT_APP.ViewModels.MusicVM;

namespace VLC_WINRT_APP.Commands.Music
{
    public class ChangeAlbumArtCommand : AlwaysExecutableCommand
    {
        public async override void Execute(object parameter)
        {
            var album = parameter as AlbumItem;

            if (album == null)
            {
                var args = parameter as ItemClickEventArgs;
                if(args != null)
                album = args.ClickedItem as AlbumItem;
            }

            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".gif");
            // Windows Phone launches the picker, then freezes the app. We need
            // to pick it up again on OnActivated.
#if WINDOWS_PHONE_APP
            App.OpenFilePickerReason = OpenFilePickerReason.OnPickingAlbumArt;
            App.SelectedAlbumItem = album;
            openPicker.PickSingleFileAndContinue();
#endif

#if WINDOWS_APP
            var file = await openPicker.PickSingleFileAsync();
            if (file == null) return;
            var byteArray = await ConvertImage.ConvertImagetoByte(file);
            await App.MusicMetaService.SaveAlbumImageAsync(album, byteArray);
            await Locator.MusicLibraryVM._albumDataRepository.Update(album);
#endif
        }
    }
}
