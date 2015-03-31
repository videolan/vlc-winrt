using System;
using System.Collections.Generic;
using System.Text;

namespace VLC_WinRT.DataRepository
{
    interface IDataRepository
    {
        void Initialize();
        void Drop();
    }
}
