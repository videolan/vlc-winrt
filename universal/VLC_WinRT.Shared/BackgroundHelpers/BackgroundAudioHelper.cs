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

#if WINDOWS_PHONE_APP
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
#if WINDOWS_PHONE_APP
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

        public async Task AddToPlaylist(List<BackgroundTrackItem> trackItems)
        {
            var bgTracks = trackItems.Select(backgroundTrackItem => new BackgroundTrackItem(backgroundTrackItem.Id, backgroundTrackItem.AlbumId, backgroundTrackItem.ArtistId, backgroundTrackItem.ArtistName, backgroundTrackItem.AlbumName, backgroundTrackItem.Name, backgroundTrackItem.Path)).ToList();
            await Locator.MediaPlaybackViewModel.BackgroundTrackRepository.AddBunchTracks(bgTracks);
#if WINDOWS_PHONE_APP
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
            Locator.MediaPlaybackViewModel.BackgroundTrackRepository.Clear();
#if WINDOWS_PHONE_APP
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
