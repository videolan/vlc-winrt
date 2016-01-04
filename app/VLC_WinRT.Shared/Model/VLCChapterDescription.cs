using libVLCX;
using System;

namespace VLC_WinRT.Model
{
    public class VLCChapterDescription
    {
        public string Name { get; set; }
        public TimeSpan StarTime { get; set; }
        public TimeSpan Duration { get; set; }

        public VLCChapterDescription(ChapterDescription chapter)
        {
            Name = chapter.name();
            StarTime = TimeSpan.FromMilliseconds(chapter.startTime());
            Duration = TimeSpan.FromMilliseconds(chapter.duration());
        }
    }
}
