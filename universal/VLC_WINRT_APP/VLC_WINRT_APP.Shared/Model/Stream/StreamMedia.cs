using System;
using Windows.Storage;
using SQLite;
using VLC_WINRT_APP.Common;

namespace VLC_WINRT_APP.Model.Stream
{
    class StreamMedia : BindableBase, IVLCMedia
    {
        private string _filePath;
        private TimeSpan _duration;
        private string _title;

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

        public StorageFile File
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public StreamMedia(string mrl)
        {
            Path = mrl;
        }
    }
}
