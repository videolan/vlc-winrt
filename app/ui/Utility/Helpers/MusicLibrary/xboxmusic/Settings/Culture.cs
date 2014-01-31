namespace XboxMusicLibrary.Settings
{
    public class Culture
    {
        public string Language { get; set; }
        public string Country { get; set; }

        public Culture(string language, string country)
        {
            this.Language = language;
            this.Country = country;
        }
    }
}
