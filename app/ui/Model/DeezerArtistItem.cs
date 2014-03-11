namespace VLC_WINRT.Model
{
    public class DeezerArtistItem
    {
        public class RootObject
        {
            public Data[] data { get; set; }
        }

        public class Data
        {
            public int id { get; set; }
            public string name { get; set; }
            public string link { get; set; }
            public string picture { get; set; }
            public int nb_album { get; set; }
            public int nb_fan { get; set; }
            public bool radio { get; set; }
            public string type { get; set; }
        }
    }
}
