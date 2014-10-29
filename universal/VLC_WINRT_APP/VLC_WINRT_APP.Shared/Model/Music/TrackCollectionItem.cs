using SQLite;

namespace VLC_WINRT_APP.Model.Music
{
    public class TrackCollectionItem
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
