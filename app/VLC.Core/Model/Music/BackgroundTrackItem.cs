using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace VLC.Model.Music
{
    public class BackgroundTrackItem
    {
        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        public int TrackId { get; set; }
    }
}
