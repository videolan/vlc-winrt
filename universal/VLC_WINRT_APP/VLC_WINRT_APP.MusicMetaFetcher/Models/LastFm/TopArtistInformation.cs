using Newtonsoft.Json;

namespace VLC_WINRT_APP.MusicMetaFetcher.Models.LastFm
{
    public class TopArtistInformation
    {
        [JsonProperty("topartists")]
        public Topartists topartists { get; set; }
    }

    public class Topartists
    {
        public TopArtistArtist[] artist { get; set; }
        public TopAttr attr { get; set; }
    }

    public class TopAttr
    {
        public string tag { get; set; }
    }

    public class TopArtistArtist
    {
        public string name { get; set; }
        public string mbid { get; set; }
        public string url { get; set; }
        public string streamable { get; set; }
        public TopArtistImage[] image { get; set; }
        public TopAttr1 attr { get; set; }
    }

    public class TopAttr1
    {
        public string rank { get; set; }
    }

    public class TopArtistImage
    {
        public string text { get; set; }
        public string size { get; set; }
    }

}
