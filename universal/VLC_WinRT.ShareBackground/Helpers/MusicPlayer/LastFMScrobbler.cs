using IF.Lastfm.Core.Api;
using IF.Lastfm.Core.Objects;
using IF.Lastfm.Core.Scrobblers;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;

namespace VLC_WinRT.SharedBackground.Helpers.MusicPlayer
{
    public sealed class LastFMScrobbler
    {
        private LastAuth _auth;
        public bool IsConnected { get; private set; }

        public LastFMScrobbler(string apiKey, string apiSig)
        {
            _auth = new LastAuth(apiKey, apiSig);
        }

        public IAsyncOperation<bool> ConnectOperation(string account, string password)
        {
            return Connect(account, password).AsAsyncOperation();
        }

        internal async Task<bool> Connect(string account, string password)
        {
            var lastResponse = await _auth.GetSessionTokenAsync(account, password);
            IsConnected = lastResponse.Success && _auth.Authenticated;
            return IsConnected;
        }

        public async void ScrobbleTrack(string artist, string album, string track)
        {
            var trackApi = new TrackApi(_auth);
            var scrobble = new Scrobble(artist, album, track, DateTimeOffset.Now);
            IScrobbler _scrobbler;
            _scrobbler = new Scrobbler(_auth);
            var response = await _scrobbler.ScrobbleAsync(scrobble);
            if (response.Success)
            {
                Debug.WriteLine("Scrobble success!");
            }
            else
            {
                Debug.WriteLine("Scrobble failed!");
            }
        }
    }
}
