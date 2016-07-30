using System;
using Windows.Storage;
using SQLite;
using VLC.Utils;
using libVLCX;
using VLC.Commands;
using VLC.Commands.VideoLibrary;
using VLC.ViewModels;
using VLC.Commands.StreamsLibrary;

namespace VLC.Model.Stream
{
    public class StreamMedia : BindableBase, IMediaItem
    {
        private string _filePath;
        private TimeSpan _duration;
        private string _title;
        private bool _favorite;

        [PrimaryKey, AutoIncrement, Column("_id")]
        public int Id { get; set; }

        [Unique]
        public string Path
        {
            get { return _filePath; }
            set { SetProperty(ref _filePath, value); }
        }
        public string Name
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public TimeSpan Duration
        {
            get { return _duration; }
            set { SetProperty(ref _duration, value); }
        }

        public bool Favorite
        {
            get { return _favorite; }
            set { SetProperty(ref _favorite, value); }
        }

        public int Order => Favorite ? 0 : 1; // TODO : nopenopenope

        [Ignore]
        public StorageFile File
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        [Ignore]
        public string Token
        {
            get
            {
                throw new NotImplementedException();
            }
            set { throw new NotImplementedException();}
        }
        
        public static DeleteStreamCommand DeleteStream { get; } = new DeleteStreamCommand();

        public static FavoriteStreamCommand FavoriteStream { get; } = new FavoriteStreamCommand();

        [Ignore]
        public Media VlcMedia { get; set; }

        public StreamMedia()
        {
            
        }

        public StreamMedia(string mrl)
        {
            Path = mrl;
        }

        public Tuple<FromType, string> GetMrlAndFromType(bool preferToken = false)
        {
            // Using a Mrl
            // FromLocation : 1
            return new Tuple<FromType, string>(FromType.FromLocation, Path);
        }
        
        public bool IsCurrentPlaying()
        {
            throw new NotImplementedException();
        }
    }
}
