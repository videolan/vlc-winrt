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
using VLC_WINRT_APP.Utility.Helpers.MusicLibrary.MusicEntities;

namespace VLC_WINRT_APP.Utility.Helpers.MusicLibrary
{
    public interface IMusicInformationManager
    {
        Task<Artist> GetArtistInfo(string artistName);

        Task<List<Artist>> GetSimilarArtists(string artistName);

        Task<Album> GetAlbumInfo(string albumTitle);

        Task<List<Album>> GetArtistTopAlbums(string artistName);
    }
}
