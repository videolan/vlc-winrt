using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using VLC.Model.Music;
using System.Collections.Generic;
using VLC.Utils;
using VLC.ViewModels;

namespace VLC.Database
{
    public class TrackDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.MusicDatabase;
        private SQLiteConnectionWithLock _connection;

        private SQLiteConnectionWithLock Connection => _connection ?? (_connection = new SQLiteConnectionWithLock(new SQLiteConnectionString(DbPath, false),
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache));

        public TrackDatabase()
        {
            Initialize();
        }

        public void Initialize()
        {
            using (Connection.Lock())
            {
                Connection.CreateTable<TrackItem>();
            }
        }

        public void Drop()
        {
            using (Connection.Lock())
            {
                Connection.DropTable<TrackItem>();
            }
        }

        public void DeleteAll()
        {
            using (Connection.Lock())
            {
                Connection.DeleteAll<TrackItem>();
            }
        }

        public bool DoesTrackExist(string path)
        {
            using (Connection.Lock())
            {
                var query = Connection.Table<TrackItem>().Where(x => x.Path == path);
                return query.Count() != 0;
            }
        }

        public TrackItem LoadTrackByPath(string path)
        {
            using (Connection.Lock())
            {
                var query = Connection.Table<TrackItem>().Where(x => x.Path == path);
                if (query.Count() > 0)
                {
                    var track = query.FirstOrDefault();
                    return track;
                }
                return null;
            }
        }

        public TrackItem LoadTrack(int trackId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Where(x => x.Id.Equals(trackId)).FirstOrDefault();
            }
        }

        public TrackItem LoadTrack(int artistId, int albumId, string trackName)
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Where(x => x.Name.Equals(trackName)).Where(x => x.ArtistId == artistId).Where(x => x.AlbumId == albumId).FirstOrDefault();
            }
        }

        public List<TrackItem> LoadTracksByAlbumId(int albumId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Where(x => x.AlbumId == albumId).OrderBy(x => x.DiscNumber).ThenBy(x => x.Index).ToList();
            }
        }

        public TrackItem GetFirstTrackOfAlbumId(int albumId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Where(x => x.AlbumId == albumId).FirstOrDefault();
            }
        }

        public List<TrackItem> LoadTracksByArtistId(int artistId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Where(x => x.ArtistId == artistId).ToList();
            }
        }

        public bool IsEmpty()
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Count() == 0;
            }
        }

        public List<TrackItem> LoadTracks()
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().OrderBy(x => x.Name).ToList();
            }
        }

        public void Update(TrackItem track)
        {
            using (Connection.Lock())
            {
                Connection.Update(track);
            }
        }

        public void Add(TrackItem track)
        {
            using (Connection.Lock())
            {
                var query = Connection.Table<TrackItem>().Where(x => x.Path == track.Path);
                var result = query.ToList();
                if (result.Count() == 0)
                    Connection.Insert(track);
            }
        }

        public void Remove(TrackItem track)
        {
            using (Connection.Lock())
            {
                Connection.Delete(track);
            }
        }
    }
}
