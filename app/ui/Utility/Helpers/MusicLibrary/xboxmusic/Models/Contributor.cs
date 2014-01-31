namespace XboxMusicLibrary.Models
{
    public class Contributor
    {
        // The contributing artist.
        public Artist Artist { get; set; }
        // The type of contribution, for example "Main" or "Featured".
        public string Role { get; set; }
    }
}
