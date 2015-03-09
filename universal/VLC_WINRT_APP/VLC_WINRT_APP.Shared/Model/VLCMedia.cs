using System;
using System.Collections.Generic;
using System.Text;

namespace VLC_WINRT_APP.Model
{
    public interface IVLCMedia
    {
        int Id { get; set; }
        string Path { get; set; }
        string Name { get; set; }
        TimeSpan Duration { get; set; }
    }
}
