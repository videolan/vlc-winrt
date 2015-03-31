using System;
using SQLite;

namespace VLC_WinRT.Model
{
    public class VLCFolder
    {
        [PrimaryKey]
        public string Path { get; set; }
        public DateTimeOffset LastModified { get; set; }

        public VLCFolder(string path, DateTimeOffset lastModif)
        {
            Path = path;
            LastModified = lastModif;
        }

        public VLCFolder()
        {
            
        }
    }
}
