/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using Windows.UI.Notifications;
using VLC_WINRT_APP.ViewModels;

namespace VLC_WINRT_APP.Helpers
{
    public class UpdateTileHelper
    {
        public static void UpdateMediumTileWithMusicInfo()
        {
            const TileTemplateType template = TileTemplateType.TileSquarePeekImageAndText02;
            var tileXml = TileUpdateManager.GetTemplateContent(template);

            var tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = "playing";
            if (Locator.MusicPlayerVM.CurrentTrack != null)
            {
                tileTextAttributes[1].InnerText = Locator.MusicPlayerVM.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.CurrentTrack.ArtistName;

                var tileImgAttribues = tileXml.GetElementsByTagName("image");
                if(Locator.MusicPlayerVM.CurrentAlbum != null)
                    tileImgAttribues[0].Attributes[1].NodeValue = Locator.MusicPlayerVM.CurrentAlbum.Picture;
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
            if (Locator.MusicPlayerVM.CurrentTrack != null)
            {
                tileTextAttributes[1].InnerText = Locator.MusicPlayerVM.CurrentTrack.Name + " - " + Locator.MusicPlayerVM.CurrentTrack.ArtistName;

                var tileImgAttribues = tileXml.GetElementsByTagName("image");
                if (Locator.MusicPlayerVM.CurrentArtist != null)
                    tileImgAttribues[0].Attributes[1].NodeValue = Locator.MusicPlayerVM.CurrentArtist.Picture;

                if (Locator.MusicPlayerVM.CurrentAlbum != null)
                    tileImgAttribues[1].Attributes[1].NodeValue = Locator.MusicPlayerVM.CurrentAlbum.Picture;
            }

            var tileNotification = new TileNotification(tileXml);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }
    }
}
