using SQLite;

namespace VLC_WINRT_APP.Model.Music
{
    public class TracklistItem
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }
        public int TrackCollectionId { get; set; }
    }
}
