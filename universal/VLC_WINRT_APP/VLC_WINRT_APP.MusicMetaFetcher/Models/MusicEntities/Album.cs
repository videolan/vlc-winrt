/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using VLC_WINRT_APP.MusicMetaFetcher.Models.LastFm;

namespace VLC_WINRT_APP.MusicMetaFetcher.Models.MusicEntities
{
    /// <summary>
    /// Entitiy representing an album.
    /// </summary>
    public class Album
    {
        public string Name { get; set; }

        public Artist Artist { get; set; }

        public string Url { get; set; }

        public List<Image> Images { get; set; }

        public long Listeners { get; set; }

        public long Playcount { get; set; }

        public void MapFrom(TopAlbum topAlbum)
        {
            this.Name = topAlbum.Name;
            this.Url = topAlbum.Url;
            this.Playcount = Convert.ToInt64(topAlbum.Playcount);

            var artist = new Artist();
            artist.MapFrom(topAlbum.Artist);
            this.Artist = artist;
            this.Images = new List<Image>();
            foreach (var image in topAlbum.Image)
            {
                var artistImage = new Image();
                artistImage.MapFrom(image);
                this.Images.Add(artistImage);
            }
        }

        public void MapFrom(Deezer.Album deezerAlbum)
        {
            this.Name = deezerAlbum.Title;
            var smallImage = new Image() { Url = string.Format("{0}?size=small", deezerAlbum.Cover) };
            var mediumImage = new Image() { Url = string.Format("{0}?size=medium", deezerAlbum.Cover) };
            var bigImage = new Image() { Url = string.Format("{0}?size=big", deezerAlbum.Cover) };
            this.Images = new List<Image>() { smallImage, mediumImage, bigImage };
            this.Url = deezerAlbum.Link;
        }

        public void MapFrom(LastFm.Album lastFmAlbum)
        {
            Image img = new Image();
            img.MapFrom(lastFmAlbum.Image.FirstOrDefault(x=>x.Size == "extralarge" || x.Size == "mega"));
            this.Images = new List<Image>() { img };
        }
    }
}
