using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VLC.Database
{
    public static class MusicDatabase
    {
        public static readonly SemaphoreSlim DatabaseOperation = new SemaphoreSlim(1);
    }
}
