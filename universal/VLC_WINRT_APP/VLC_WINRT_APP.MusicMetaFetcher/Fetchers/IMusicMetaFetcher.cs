/**********************************************************************
 * VLC for WinRT
 **********************************************************************
 * Copyright © 2013-2014 VideoLAN and Authors
 *
 * Licensed under GPLv2+ and MPLv2
 * Refer to COPYING file of the official project for license
 **********************************************************************/

using System.Collections.Generic;
using System.Threading.Tasks;
using VLC_WinRT.MusicMetaFetcher.Models.MusicEntities;

namespace VLC_WinRT.MusicMetaFetcher.Fetchers
{
    public interface IMusicMetaFetcher
    {
        Task<Artist> GetArtistInfo(string artistName);
        Task<List<Artist>> GetSimilarArtists(string artistName);
        Task<Album> GetAlbumInfo(string albumTitle, string artistName);
        Task<List<Album>> GetArtistTopAlbums(string artistName);
        Task<List<Artist>> GetTopArtistsGenre(string genre);
    }
}
