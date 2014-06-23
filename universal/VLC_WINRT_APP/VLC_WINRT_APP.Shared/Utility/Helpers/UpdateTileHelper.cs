/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Notifications;
using VLC_WINRT.ViewModels;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Utility.Helpers
{
    public class UpdateTileHelper
    {
        public static void UpdateMediumTileWithMusicInfo()
        {
            const TileTemplateType template = TileTemplateType.TileSquarePeekImageAndText02;
            var tileXml = TileUpdateManager.GetTemplateContent(template);

            var tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = "playing";
            if (Locator.MusicPlayerVM.Artist != null)
            {
                tileTextAttributes[1].InnerText = Locator.MusicPlayerVM.Artist.CurrentAlbumItem.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.Artist.CurrentAlbumItem.Artist;

                var tileImgAttribues = tileXml.GetElementsByTagName("image");
                tileImgAttribues[0].Attributes[1].NodeValue = Locator.MusicPlayerVM.Artist.CurrentAlbumItem.Picture;
            }

            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            UpdateBigTileWithMusicInfo();
        }

        public static void UpdateBigTileWithMusicInfo()
        {
            const TileTemplateType template = TileTemplateType.TileWidePeekImage05;
            var tileXml = TileUpdateManager.GetTemplateContent(template);

            var tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = "Now playing";
            if (Locator.MusicPlayerVM.Artist != null)
            {
                tileTextAttributes[1].InnerText = Locator.MusicPlayerVM.Artist.CurrentAlbumItem.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.Artist.CurrentAlbumItem.Artist;

                var tileImgAttribues = tileXml.GetElementsByTagName("image");
                tileImgAttribues[0].Attributes[1].NodeValue = Locator.MusicPlayerVM.Artist.Picture;
                tileImgAttribues[1].Attributes[1].NodeValue = Locator.MusicPlayerVM.Artist.CurrentAlbumItem.Picture;
            }

            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }
    }
}
