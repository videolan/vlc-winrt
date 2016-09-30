using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VLC.Model.Music;
using VLC.Utils;

namespace VLC.Database
{
    public class MusicDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.MusicDatabase;
        private static SQLiteConnectionWithLock _connection;

        private static SQLiteConnectionWithLock Connection => _connection ?? (_connection = new SQLiteConnectionWithLock(new SQLiteConnectionString(DbPath, false),
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache));

        public void Initialize()
        {
            using (Connection.Lock())
            {
                Connection.CreateTable<ArtistItem>();
                Connection.CreateTable<AlbumItem>();
                Connection.CreateTable<TrackItem>();
            }
        }

        public void DeleteAll()
        {
            using (Connection.Lock())
            {
                Connection.DeleteAll<ArtistItem>();
                Connection.DeleteAll<AlbumItem>();
                Connection.DeleteAll<TrackItem>();
            }
        }

        public void Drop()
        {
            using (Connection.Lock())
            {
                Connection.DropTable<ArtistItem>();
                Connection.DropTable<AlbumItem>();
                Connection.DropTable<TrackItem>();
            }
        }

        public bool IsEmpty()
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Count() == 0;
            }
        }

        #region load artists
        public ArtistItem LoadArtistFromId(int artistId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<ArtistItem>().Where(x => x.Id.Equals(artistId)).FirstOrDefault();
            }
        }

        public List<ArtistItem> LoadArtists(Expression<Func<ArtistItem, bool>> compare = null)
        {
            using (Connection.Lock())
            {
                TableQuery<ArtistItem> query;
                if (compare == null)
                {
                    query = Connection.Table<ArtistItem>();
                }
                else
                {
                    query = Connection.Table<ArtistItem>().Where(compare);
                }
                var artists = query.ToList();
                return artists;
            }
        }

        public ArtistItem LoadFromArtistName(string artistName)
        {
            using (Connection.Lock())
            {
                return Connection.Table<ArtistItem>().Where(x => x.Name.Equals(artistName)).FirstOrDefault();
            }
        }

        #endregion
        #region load albums
        public AlbumItem LoadAlbumFromId(int albumId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<AlbumItem>().Where(x => x.Id.Equals(albumId)).FirstOrDefault();
            }
        }

        /// <summary>
        /// This method gets the albums for a specified artist, along with all the tracks of the albums
        /// TODO : The SQLite request could be greatly improved IMO
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns></returns>
        public List<AlbumItem> LoadAlbumsFromIdWithTracks(int artistId)
        {
            using (Connection.Lock())
            {
                var albums = Connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).OrderBy(x => x.Name).ToList();
                foreach (var album in albums)
                {
                    var tracks = Connection.Table<TrackItem>().Where(x => x.AlbumId == album.Id).OrderBy(x => x.DiscNumber).ThenBy(x => x.Index).ToList();
                    album.Tracks = tracks;
                }
                return albums;
            }
        }

        public int LoadAlbumsCountFromId(int artistId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).Count();
            }
        }

        public AlbumItem LoadAlbumFromName(int artistId, string albumName)
        {
            using (Connection.Lock())
            {
                return Connection.Table<AlbumItem>().Where(x => x.Name.Equals(albumName)).Where(x => x.ArtistId == artistId).FirstOrDefault();
            }
        }


        public List<AlbumItem> LoadAlbums(Expression<Func<AlbumItem, bool>> compare = null)
        {
            using (Connection.Lock())
            {
                TableQuery<AlbumItem> query;
                if (compare == null)
                {
                    query = Connection.Table<AlbumItem>();
                }
                else
                {
                    query = Connection.Table<AlbumItem>().Where(compare);
                }
                return query.ToList();
            }
        }

        public List<AlbumItem> Load(Expression<Func<AlbumItem, bool>> compare = null)
        {
            using (Connection.Lock())
            {
                TableQuery<AlbumItem> query;
                if (compare == null)
                {
                    query = Connection.Table<AlbumItem>();
                }
                else
                {
                    query = Connection.Table<AlbumItem>().Where(compare);
                }
                return query.ToList();
            }
        }

        public List<AlbumItem> LoadAlbumsFromArtistId(int artistId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).OrderBy(x => x.Name).ToList();
            }
        }

        public List<AlbumItem> LoadAlbumsFromColumnValue(string column, string value)
        {
            using (Connection.Lock())
            {
                return Connection.Query<AlbumItem>($"SELECT * FROM {nameof(AlbumItem)} WHERE {column} LIKE '%{value}%';", new string[] { });
            }
        }
        #endregion

        #region load tracks

        public TrackItem LoadTrackFromId(int trackId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Where(x => x.Id.Equals(trackId)).FirstOrDefault();
            }
        }

        public List<TrackItem> LoadTracks()
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().OrderBy(x => x.Name).ToList();
            }
        }

        public TrackItem LoadTrackFromPath(string path)
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

        public List<TrackItem> LoadTracksFromAlbumId(int albumId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Where(x => x.AlbumId == albumId).OrderBy(x => x.DiscNumber).ThenBy(x => x.Index).ToList();
            }
        }

        public List<TrackItem> LoadTracksFromArtistId(int artistId)
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Where(x => x.ArtistId == artistId).ToList();
            }
        }
        #endregion

        #region contains

        public bool ContainsTrack(string path)
        {
            using (Connection.Lock())
            {
                return Connection.Table<TrackItem>().Where(x => x.Path == path).Count() != 0;
            }
        }
        #endregion
        #region update
        public void Update(ArtistItem artist)
        {
            using (Connection.Lock())
            {
                Connection.Update(artist);
            }
        }


        public void Update(AlbumItem album)
        {
            using (Connection.Lock())
            {
                Connection.Update(album);
            }
        }

        public void Update(TrackItem track)
        {
            using (Connection.Lock())
            {
                Connection.Update(track);
            }
        }
        #endregion
        #region add
        public void Add(ArtistItem artist)
        {
            using (Connection.Lock())
            {
                Connection.Insert(artist);
            }
        }

        public void Add(AlbumItem album)
        {
            using (Connection.Lock())
            {
                Connection.Insert(album);
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

        #endregion
        #region remove

        public void Remove(ArtistItem artist)
        {
            using (Connection.Lock())
            {
                Connection.Delete(artist);
            }
        }

        public void Remove(AlbumItem album)
        {
            if (album == null)
                return;
            using (Connection.Lock())
                Connection.Delete(album);
        }

        public void Remove(TrackItem track)
        {
            using (Connection.Lock())
            {
                Connection.Delete(track);
            }
        }
        #endregion

        public int ArtistsCount()
        {
            using (Connection.Lock())
            {
                return Connection.Table<ArtistItem>().Count();
            }
        }

        public ArtistItem ArtistAt(int index)
        {
            using (Connection.Lock())
            {
                return Connection.Table<ArtistItem>().ElementAt(index);
            }
        }
    }
}
