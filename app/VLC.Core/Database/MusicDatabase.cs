using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VLC.Model;
using VLC.Model.Music;
using VLC.Utils;

namespace VLC.Database
{
    public class MusicDatabase : IDatabase
    {
        private static readonly string DbPath = Strings.MusicDatabase;

        private static SQLiteConnectionWithLock connection = new SQLiteConnectionWithLock(new SQLiteConnectionString(DbPath, false),
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.FullMutex | SQLiteOpenFlags.SharedCache);

        public void Initialize()
        {
            using (connection.Lock())
            {
                connection.CreateTable<ArtistItem>();
                connection.CreateTable<AlbumItem>();
                connection.CreateTable<TrackItem>();
                connection.CreateTable<TracklistItem>();
                connection.CreateTable<PlaylistItem>();
            }
        }

        public void DeleteAll()
        {
            using (connection.Lock())
            {
                connection.DeleteAll<ArtistItem>();
                connection.DeleteAll<AlbumItem>();
                connection.DeleteAll<TrackItem>();
                connection.DeleteAll<TracklistItem>();
                connection.DeleteAll<PlaylistItem>();
            }
        }

        public void Drop()
        {
            using (connection.Lock())
            {
                connection.DropTable<ArtistItem>();
                connection.DropTable<AlbumItem>();
                connection.DropTable<TrackItem>();
                connection.DropTable<TracklistItem>();
                connection.DropTable<PlaylistItem>();
            }
        }

        #region AlbumItem
        public AlbumItem LoadAlbumFromId(int albumId)
        {
            using (connection.Lock())
            {
                return connection.Table<AlbumItem>().Where(x => x.Id.Equals(albumId)).FirstOrDefault();
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
            using (connection.Lock())
            {
                var albums = connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).OrderBy(x => x.Name).ToList();
                foreach (var album in albums)
                {
                    var tracks = connection.Table<TrackItem>().Where(x => x.AlbumId == album.Id).OrderBy(x => x.DiscNumber).ThenBy(x => x.Index).ToList();
                    album.Tracks = tracks;
                }
                return albums;
            }
        }

        public int LoadAlbumsCountFromId(int artistId)
        {
            using (connection.Lock())
            {
                return connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).Count();
            }
        }

        public AlbumItem LoadAlbumFromName(int artistId, string albumName)
        {
            using (connection.Lock())
            {
                return connection.Table<AlbumItem>().Where(x => x.Name.Equals(albumName)).Where(x => x.ArtistId == artistId).FirstOrDefault();
            }
        }

        public List<AlbumItem> LoadAlbums(Expression<Func<AlbumItem, bool>> compare = null)
        {
            using (connection.Lock())
            {
                TableQuery<AlbumItem> query;
                if (compare == null)
                {
                    query = connection.Table<AlbumItem>();
                }
                else
                {
                    query = connection.Table<AlbumItem>().Where(compare);
                }
                return query.ToList();
            }
        }

        public List<AlbumItem> Load(Expression<Func<AlbumItem, bool>> compare = null)
        {
            using (connection.Lock())
            {
                TableQuery<AlbumItem> query;
                if (compare == null)
                {
                    query = connection.Table<AlbumItem>();
                }
                else
                {
                    query = connection.Table<AlbumItem>().Where(compare);
                }
                return query.ToList();
            }
        }

        public List<AlbumItem> LoadAlbumsFromArtistId(int artistId)
        {
            using (connection.Lock())
            {
                return connection.Table<AlbumItem>().Where(x => x.ArtistId == artistId).OrderBy(x => x.Name).ToList();
            }
        }

        public List<AlbumItem> LoadAlbumsFromColumnValue(string column, string value)
        {
            using (connection.Lock())
            {
                return connection.Query<AlbumItem>($"SELECT * FROM {nameof(AlbumItem)} WHERE {column} LIKE '%{value}%';", new string[] { });
            }
        }

        public void Add(AlbumItem album)
        {
            using (connection.Lock())
            {
                connection.Insert(album);
            }
        }

        public void Update(AlbumItem album)
        {
            using (connection.Lock())
            {
                connection.Update(album);
            }
        }

        public void Remove(AlbumItem album)
        {
            if (album == null)
                return;
            using (connection.Lock())
                connection.Delete(album);
        }
        #endregion

        #region TrackItem
        public TrackItem LoadTrackFromId(int trackId)
        {
            using (connection.Lock())
            {
                return connection.Table<TrackItem>().FirstOrDefault(x => x.Id.Equals(trackId));
            }
        }

        public List<TrackItem> LoadTracks()
        {
            using (connection.Lock())
            {
                return connection.Table<TrackItem>().OrderBy(x => x.Name).ToList();
            }
        }

        public TrackItem LoadTrackFromPath(string path)
        {
            using (connection.Lock())
            {
                return connection.Table<TrackItem>().FirstOrDefault(x => x.Path == path);
            }
        }

        public List<TrackItem> LoadTracksFromAlbumId(int albumId)
        {
            using (connection.Lock())
            {
                return connection.Table<TrackItem>().Where(x => x.AlbumId == albumId).OrderBy(x => x.DiscNumber).ThenBy(x => x.Index).ToList();
            }
        }

        public List<TrackItem> LoadTracksFromArtistId(int artistId)
        {
            using (connection.Lock())
            {
                return connection.Table<TrackItem>().Where(x => x.ArtistId == artistId).ToList();
            }
        }

        public bool HasNoTrack()
        {
            using (connection.Lock())
            {
                return !connection.Table<TrackItem>().Any();
            }
        }

        public bool ContainsTrack(string path)
        {
            using (connection.Lock())
            {
                return connection.Table<TrackItem>().Count(x => x.Path == path) != 0;
            }
        }

        public void Add(TrackItem track)
        {
            using (connection.Lock())
            {
                var query = connection.Table<TrackItem>().Where(x => x.Path == track.Path);
                var result = query.ToList();
                if (!result.Any())
                    connection.Insert(track);
            }
        }

        public void Update(TrackItem track)
        {
            using (connection.Lock())
            {
                connection.Update(track);
            }
        }

        public void Remove(TrackItem track)
        {
            using (connection.Lock())
            {
                connection.Delete(track);
            }
        }
        #endregion

        #region ArtistItem
        public ArtistItem LoadArtistFromId(int artistId)
        {
            using (connection.Lock())
            {
                return connection.Table<ArtistItem>().FirstOrDefault(x => x.Id.Equals(artistId));
            }
        }

        public List<ArtistItem> LoadArtists(Expression<Func<ArtistItem, bool>> compare = null)
        {
            using (connection.Lock())
            {
                TableQuery<ArtistItem> query;
                query = compare == null ? connection.Table<ArtistItem>() : connection.Table<ArtistItem>().Where(compare);
                var artists = query.ToList();
                return artists;
            }
        }

        public ArtistItem LoadFromArtistName(string artistName)
        {
            using (connection.Lock())
            {
                return connection.Table<ArtistItem>().FirstOrDefault(x => x.Name.Equals(artistName));
            }
        }

        public int ArtistsCount()
        {
            using (connection.Lock())
            {
                return connection.Table<ArtistItem>().Count();
            }
        }

        public ArtistItem ArtistAt(int index)
        {
            using (connection.Lock())
            {
                return connection.Table<ArtistItem>().ElementAt(index);
            }
        }

        public void Add(ArtistItem artist)
        {
            using (connection.Lock())
            {
                connection.Insert(artist);
            }
        }

        public void Update(ArtistItem artist)
        {
            using (connection.Lock())
            {
                connection.Update(artist);
            }
        }

        public void Remove(ArtistItem artist)
        {
            using (connection.Lock())
            {
                connection.Delete(artist);
            }
        }
        #endregion

        #region TracklistItem
        public List<TracklistItem> LoadTracks(PlaylistItem trackCollection)
        {
            using (connection.Lock())
            {
                return connection.Table<TracklistItem>().Where(x => x.TrackCollectionId == trackCollection.Id).ToList();
            }
        }

        public void Add(TracklistItem track)
        {
            using (connection.Lock())
            {
                connection.Insert(track);
            }
        }

        public void Remove(TracklistItem track)
        {
            using (connection.Lock())
            {
                connection.Delete(track);
            }
        }

        public void RemoveTracklistItemWithIds(int trackId, int trackCollectionId)
        {
            using (connection.Lock())
            {
                connection.Execute("DELETE FROM TracklistItem WHERE TrackCollectionId=? AND TrackId=?;", trackCollectionId, trackId);
            }
        }

        public void Clear()
        {
            using (connection.Lock())
            {
                connection.Execute("DELETE FROM TracklistItem");
            }
        }
        #endregion

        #region PlaylistItem
        public PlaylistItem LoadPlayListItemFromName(string name)
        {
            using (connection.Lock())
            {
                return connection.Table<PlaylistItem>().FirstOrDefault(x => x.Name == name);
            }
        }

        public List<PlaylistItem> LoadTrackCollections()
        {
            using (connection.Lock())
            {
                return connection.Table<PlaylistItem>().ToList();
            }
        }

        public void Add(PlaylistItem trackCollection)
        {
            using (connection.Lock())
            {
                connection.Insert(trackCollection);
            }
        }

        public void Remove(PlaylistItem trackCollection)
        {
            using (connection.Lock())
            {
                connection.Delete(trackCollection);
            }
        }
        #endregion
    }
}
