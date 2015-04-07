using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.BackgroundAudioPlayer;
using VLC_WinRT.BackgroundAudioPlayer.Model;
using VLC_WinRT.ViewModels;
using Windows.Foundation.Collections;
#if WINDOWS_PHONE_APP
using Windows.Media.Playback;
#endif
namespace VLC_WinRT.BackgroundHelpers
{
    public class BackgroundAudioHelper
    {
        public void RestorePlaylist()
        {
#if WINDOWS_PHONE_APP
            var msgDictionanary = new ValueSet();
            msgDictionanary.Add(BackgroundAudioConstants.RestorePlaylist, "");
            BackgroundMediaPlayer.SendMessageToBackground(msgDictionanary);
#endif
        }

        public async Task AddToPlaylist(List<BackgroundTrackItem> trackItems)
        {
            //if (IsMyBackgroundTaskRunning)
            {
                var bgTracks = trackItems.Select(backgroundTrackItem => new BackgroundTrackItem(backgroundTrackItem.Id, backgroundTrackItem.AlbumId, backgroundTrackItem.ArtistId, backgroundTrackItem.ArtistName, backgroundTrackItem.AlbumName, backgroundTrackItem.Name, backgroundTrackItem.Path)).ToList();
                await Locator.MusicPlayerVM.BackgroundTrackRepository.AddBunchTracks(bgTracks);
#if WINDOWS_PHONE_APP
                var msgDictionary = new ValueSet();
                msgDictionary.Add(BackgroundAudioConstants.UpdatePlaylist, "");
                BackgroundMediaPlayer.SendMessageToBackground(msgDictionary);
#endif
            }
        }

        public async Task AddToPlaylist(BackgroundTrackItem trackItem)
        {
            var list = new List<BackgroundTrackItem> { trackItem };
            await AddToPlaylist(list);
        }

        public async Task ResetCollection(ResetType resetType)
        {
            //if (IsMyBackgroundTaskRunning)
            {
                Locator.MusicPlayerVM.BackgroundTrackRepository.Clear();
#if WINDOWS_PHONE_APP
                ValueSet messageDictionary = new ValueSet();
                messageDictionary.Add(BackgroundAudioConstants.ResetPlaylist, (int)resetType);
                BackgroundMediaPlayer.SendMessageToBackground(messageDictionary);
                await Task.Delay(500);
#endif
            }
        }
    }
}
