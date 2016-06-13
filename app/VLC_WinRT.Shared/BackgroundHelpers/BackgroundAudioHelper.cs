using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using VLC_WinRT.BackgroundAudioPlayer.Model;
using VLC_WinRT.ViewModels;
using Windows.Foundation.Collections;
#if TWO_PROCESS_BGA
using Windows.Media.Playback;
#endif
namespace VLC_WinRT.BackgroundHelpers
{
    public class BackgroundAudioHelper
    {

#if TWO_PROCESS_BGA
        private static MediaPlayer _instance;
        public static MediaPlayer Instance
        {
            get
            {
                try
                {
                    return _instance ?? (_instance = BackgroundMediaPlayer.Current);
                }
                catch
                {
                    return null;
                }
            }
        }
#endif

        public void RestorePlaylist()
        {
#if TWO_PROCESS_BGA
            try
            {
                var msgDictionanary = new ValueSet();
                msgDictionanary.Add(BackgroundAudioConstants.RestorePlaylist, "");
                BackgroundMediaPlayer.SendMessageToBackground(msgDictionanary);
            }
            catch
            {
            }
#endif
        }

        public async Task AddToPlaylist(IEnumerable<BackgroundTrackItem> trackItems)
        {
            var bgTracks = trackItems.Select(backgroundTrackItem => new BackgroundTrackItem(backgroundTrackItem.Id, backgroundTrackItem.AlbumId, backgroundTrackItem.ArtistId, backgroundTrackItem.ArtistName, backgroundTrackItem.AlbumName, backgroundTrackItem.Name, backgroundTrackItem.Path)).ToList();
            await Locator.MediaPlaybackViewModel.PlaybackService.BackgroundTrackRepository.AddBunchTracks(bgTracks);
#if TWO_PROCESS_BGA
            try
            {
                var msgDictionary = new ValueSet();
                msgDictionary.Add(BackgroundAudioConstants.UpdatePlaylist, "");
                BackgroundMediaPlayer.SendMessageToBackground(msgDictionary);
            }
            catch
            {
            }
#endif
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
#if TWO_PROCESS_BGA
            try
            {
                ValueSet messageDictionary = new ValueSet();
                messageDictionary.Add(BackgroundAudioConstants.ResetPlaylist, (int)resetType);
                BackgroundMediaPlayer.SendMessageToBackground(messageDictionary);
                await Task.Delay(500);
            }
            catch
            {
            }
#endif
        }
    }
}
