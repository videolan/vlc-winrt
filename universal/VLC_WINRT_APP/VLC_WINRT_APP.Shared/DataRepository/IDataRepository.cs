using System;
using System.Collections.Generic;
using System.Text;

namespace VLC_WINRT_APP.DataRepository
{
    interface IDataRepository
    {
        void Initialize();
        void Drop();
    }
}
