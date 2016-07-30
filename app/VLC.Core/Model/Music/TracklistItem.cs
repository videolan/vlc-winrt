using SQLite;

namespace VLC.Model.Music
{
    public class TracklistItem
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public int TrackId { get; set; }
        public int TrackCollectionId { get; set; }
    }
}
