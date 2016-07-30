using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.BackgroundAudioPlayer.Model;
using VLC_WinRT.ViewModels;
using Windows.Foundation.Collections;

namespace VLC_WinRT.BackgroundHelpers
{
    public class BackgroundAudioHelper
    {
        public async Task AddToPlaylist(IEnumerable<BackgroundTrackItem> trackItems)
        {
            var bgTracks = trackItems.Select(backgroundTrackItem => new BackgroundTrackItem(backgroundTrackItem.Id, backgroundTrackItem.AlbumId, backgroundTrackItem.ArtistId, backgroundTrackItem.ArtistName, backgroundTrackItem.AlbumName, backgroundTrackItem.Name, backgroundTrackItem.Path)).ToList();
            await Locator.MediaPlaybackViewModel.PlaybackService.BackgroundTrackRepository.AddBunchTracks(bgTracks);
        }

        public async Task AddToPlaylist(BackgroundTrackItem trackItem)
        {
            try
            {
                var list = new List<BackgroundTrackItem> { trackItem };
                await AddToPlaylist(list);
            }
            catch
            {
            }
        }

        public async Task ResetCollection(ResetType resetType)
        {
            Locator.MediaPlaybackViewModel.PlaybackService.BackgroundTrackRepository.Clear();
        }
    }
}
