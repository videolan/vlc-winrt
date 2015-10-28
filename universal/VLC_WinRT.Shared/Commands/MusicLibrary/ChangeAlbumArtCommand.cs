/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;
using VLC_WinRT.Model.Music;
using System;
using VLC_WinRT.ViewModels;
using VLC_WinRT.Model;
using VLC_WinRT.Utils;

namespace VLC_WinRT.Commands.MusicLibrary
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
#else
            var file = await openPicker.PickSingleFileAsync();
            if (file == null) return;
            var byteArray = await ConvertImage.ConvertImagetoByte(file);
            await Locator.MusicMetaService.SaveAlbumImageAsync(album, byteArray);
            await Locator.MusicLibraryVM._albumDatabase.Update(album);
#endif
        }
    }
}
