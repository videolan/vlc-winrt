using System;
using System.Collections.Generic;
using System.Text;

namespace VLC_WINRT_APP.Model
{
    public static class VLCFileExtensions
    {
        public static List<string> Supported = new List<string>()
        {
            ".3g2",
            ".3gp",
            ".3gp2",
            ".3gpp",
            ".amv",
            ".asf",
            ".avi",
            ".divx",
            ".drc",
            ".dv",
            ".f4v",
            ".flv",
            ".gvi",
            ".gxf",
            ".ismv",
            ".iso",
            ".m1v",
            ".m2v",
            ".m2t",
            ".m2ts",
            ".m3u8",
            ".mkv",
            ".mov",
            ".mp2",
            ".mp2v",
            ".mp4",
            ".mp4v",
            ".mpe",
            ".mpeg",
            ".mpeg1",
            ".mpeg2",
            ".mpeg4",
            ".mpg",
            ".mpv2",
            ".mts",
            ".mtv",
            ".mxf",
            ".mxg",
            ".nsv",
            ".nut",
            ".nuv",
            ".ogm",
            ".ogv",
            ".ogx",
            ".ps",
            ".rec",
            ".rm",
            ".rmvb",
            ".tob",
            ".ts",
            ".tts",
            ".vob",
            ".vro",
            ".webm",
            ".wm",
            ".wmv",
            ".wtv",
            ".xesc",
            ".mp3",
            ".ogg",
            ".aac",
            ".wma",
            ".wav",
            ".flac",
        };

        public enum VLCFileType
        {
            Audio,
            Video,
            Other,
        }

        public static VLCFileType FileTypeHelper(string ext)
        {
            throw new NotImplementedException();
        }
    }
}
