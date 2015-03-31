using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using IF.Lastfm.Core.Api;

namespace VLC_WinRT.LastFmScrobbler
{
    public sealed class LastFmScrobblerHelper
    {
        private LastAuth _auth;
        public bool IsConnected { get; private set; }

        public LastFmScrobblerHelper(string apiKey, string apiSig)
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
            var response = await trackApi.ScrobbleAsync(new Scrobble(artist, album, track, DateTimeOffset.Now));
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
